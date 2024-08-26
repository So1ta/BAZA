import logging
import asyncio
import os
import datetime
from telegram import Update, ReplyKeyboardMarkup
from telegram.ext import (
    ApplicationBuilder,
    ContextTypes,
    CommandHandler,
    MessageHandler,
    filters,
    ConversationHandler,
)
from github import Github, UnknownObjectException

# Ваши данные для GitHub
GITHUB_TOKEN = "GITHUB_TOKEN"
GITHUB_USERNAME = "GITHUB_USERNAME"
GITHUB_REPOSITORY = "GITHUB_REPOSITORY"

# Замените 'YOUR_BOT_TOKEN' на токен вашего бота
BOT_TOKEN = 'YOUR_BOT_TOKEN'

# Настройка логирования
logging.basicConfig(
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    level=logging.INFO
)

# Создание экземпляра GitHub
gh = Github(GITHUB_TOKEN)
repo = gh.get_user(GITHUB_USERNAME).get_repo(GITHUB_REPOSITORY)

# Состояния для ConversationHandler
WAITING_FOR_MESSAGE = 0

# Клавиатура с кнопками
keyboard = ReplyKeyboardMarkup([
    ["Отправить сообщение на GitHub"],
    ["Очистить репозиторий GitHub"],
], resize_keyboard=True)

# Обработчик команды /start
async def start(update: Update, context: ContextTypes.DEFAULT_TYPE):
    await update.message.reply_text("Выберите действие:", reply_markup=keyboard)

# Обработчик кнопки "Отправить сообщение на GitHub"
async def wait_for_message(update: Update, context: ContextTypes.DEFAULT_TYPE):
    await update.message.reply_text("Отправьте сообщение, которое хотите загрузить на GitHub:")
    return WAITING_FOR_MESSAGE

async def handle_message(update: Update, context: ContextTypes.DEFAULT_TYPE):
    chat_id = update.effective_chat.id
    message = update.message

    try:
        # Создание уникального имени файла на основе ID сообщения и типа контента
        file_name = f"message_{message.message_id}"
        folder_name = ""
        if message.text:
            file_name += ".txt"
            content = message.text
            folder_name = "text/"
        elif message.photo:
            file_name += ".jpg"
            file = await context.bot.get_file(message.photo[-1].file_id)
            content = bytes(await file.download_as_bytearray())
            folder_name = "images/"
        elif message.video:
            file_name += ".mp4"
            file = await context.bot.get_file(message.video.file_id)
            content = bytes(await file.download_as_bytearray())
            folder_name = "videos/"
        elif message.document:
            file_name = message.document.file_name
            file = await context.bot.get_file(message.document.file_id)
            content = bytes(await file.download_as_bytearray())

            # Проверка расширения файла для аудио
            if file_name.lower().endswith(('.mp3', '.wav', '.ogg')):
                folder_name = "audio/"
            else:  # Если это не аудио, то не обрабатываем
                await context.bot.send_message(chat_id=chat_id, text="Этот тип документа не поддерживается.")
                return
        else:
            await context.bot.send_message(chat_id=chat_id, text="Этот тип сообщения не поддерживается.")
            return

        # Формирование имени файла с датой и временем
        timestamp = datetime.datetime.now().strftime("%d_%m_%Y_%H-%M-%S")
        file_name = f"{timestamp}_{file_name}"  # Добавляем timestamp к имени файла

        # Загрузка файла на GitHub
        repo.create_file(f"{folder_name}{file_name}", "Add message from Telegram bot", content, branch="main")
        await context.bot.send_message(chat_id=chat_id, text="Сообщение успешно загружено на GitHub!")

    except Exception as e:
        logging.exception("Ошибка при загрузке сообщения:")
        await context.bot.send_message(chat_id=chat_id, text="Произошла ошибка при загрузке сообщения.")

    # Возвращаем пользователя в главное меню
    await update.message.reply_text("Выберите действие:", reply_markup=keyboard)
    return ConversationHandler.END




async def clear_repository(update: Update, context: ContextTypes.DEFAULT_TYPE):
    try:
        async def delete_contents(contents):
            """Рекурсивно удаляет файлы и папки с задержкой."""
            for content in contents:
                try:
                    if content.type == 'dir':
                        # Если это папка, рекурсивно удаляем ее содержимое
                        await delete_contents(repo.get_contents(content.path))
                        await asyncio.sleep(1)  # Задержка 1 секунда

                        # Проверяем существование папки перед удалением
                        try:
                            repo.get_contents(content.path)
                            repo.delete_file(content.path, "Clearing repository", content.sha, branch="main")
                        except UnknownObjectException:
                            logging.warning(f"Папка {content.path} не найдена, возможно, уже удалена.")
                    else:
                        # Если это файл, удаляем его
                        await asyncio.sleep(1)  # Задержка 1 секунда

                        # Проверяем существование файла перед удалением
                        try:
                            repo.get_contents(content.path)
                            repo.delete_file(content.path, "Clearing repository", content.sha, branch="main")
                        except UnknownObjectException:
                            logging.warning(f"Файл {content.path} не найден, возможно, уже удален.")

                except Exception as e:
                    # Выводим подробную информацию об ошибке
                    logging.error(f"Ошибка при удалении {content.path}: {e}")
                    await update.message.reply_text(f"Ошибка при удалении {content.path}: {e}")

        # Начинаем с корня репозитория
        await delete_contents(repo.get_contents(""))

        await update.message.reply_text("Репозиторий GitHub успешно очищен!")

    except Exception as e:
        logging.error(f"Ошибка при очистке репозитория: {e}")
        await update.message.reply_text("Произошла ошибка при очистке репозитория.")

    # Возвращаем пользователя в главное меню
    await update.message.reply_text("Выберите действие:", reply_markup=keyboard)
    return ConversationHandler.END


# Основная функция бота
if __name__ == '__main__':
    application = ApplicationBuilder().token(BOT_TOKEN).build()

    # Создание ConversationHandler для обработки кнопок и сообщений
    conv_handler = ConversationHandler(
        entry_points=[
            CommandHandler('start', start),
            MessageHandler(filters.Regex("Отправить сообщение на GitHub"), wait_for_message),
            MessageHandler(filters.Regex("Очистить репозиторий GitHub"), clear_repository),
        ],
        states={
            WAITING_FOR_MESSAGE: [MessageHandler(filters.ALL, handle_message)],
        },
        fallbacks=[],
    )

    application.add_handler(conv_handler)

    # Запускаем бота
    asyncio.run(application.run_polling())
