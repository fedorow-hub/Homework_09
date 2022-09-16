using System;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Homework09
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string token = System.IO.File.ReadAllText("token.txt");
            var botClient = new TelegramBotClient(token);

            using var cts = new CancellationTokenSource();
                        
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );
                                   
            Console.ReadLine();
                       
            cts.Cancel();            

            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {                
                if (update.Type != UpdateType.Message)
                    return;
                
                var chatId = update.Message!.Chat.Id;
                var chatUserName = update.Message!.Chat.FirstName;

                DirectoryInfo directory = new DirectoryInfo(".");
                directory.CreateSubdirectory(chatUserName!);

                if (update.Message!.Type == MessageType.Photo)
                {

                    var fileId = update.Message.Photo!.Last().FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    DirectoryInfo subDirectory = new DirectoryInfo(chatUserName!);
                    var subDir = subDirectory.CreateSubdirectory("Фото");

                    string destinationFilePath = $"{chatUserName}\\Фото\\{fileInfo.FileUniqueId}.jpg";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(
                        filePath: filePath!,
                        destination: fileStream);
                }

                if (update.Message!.Type == MessageType.Video)
                {
                    var file = update.Message.Video;
                    var fileId = file!.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    DirectoryInfo subDirectory = new DirectoryInfo(chatUserName!);
                    var subDir = subDirectory.CreateSubdirectory("Видео");

                    string destinationFilePath = $"{chatUserName}\\Видео\\{file.FileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(
                        filePath: filePath!,
                        destination: fileStream);
                }

                if (update.Message!.Type == MessageType.Document)
                {
                    var file = update.Message.Document;
                    var fileId = file!.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;

                    DirectoryInfo subDirectory = new DirectoryInfo(chatUserName!);
                    var subDir = subDirectory.CreateSubdirectory("Документы");

                    string destinationFilePath = $"{chatUserName}\\Документы\\{file.FileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(
                        filePath: filePath!,
                        destination: fileStream);
                }               


                if (update.Message.Text == "/start")
                {
                    ReplyKeyboardMarkup replyKeyboardMarkup = new(new[]
                    {
                        new KeyboardButton[] { "Фото", "Видео", "Документы" },
                    })
                    {
                        ResizeKeyboard = true
                    };

                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Выберите папку",
                        replyMarkup: replyKeyboardMarkup,
                        cancellationToken: cancellationToken);
                }                               

                string userAnswer = update.Message.Text!;

                if ( update.Message.Text == "Документы" | update.Message.Text == "Видео" | update.Message.Text == "Фото")
                {
                    string[] fullFileNames = Directory.GetFiles($".\\{new DirectoryInfo(chatUserName!)}\\{userAnswer}"); 
                    string[] fileNames = new string[fullFileNames.Length];

                    for (int i = 0; i < fullFileNames.Length; i++)
                    {
                        fileNames[i] = fullFileNames[i].Split('\\').Last();                        
                    }

                    var ilist = new List<InlineKeyboardButton[]>();
                                        
                    for (int i = 0; i < fileNames.Length; i++)
                    {
                        var inlineKeyboardButtons = new List<InlineKeyboardButton>()
                        {
                            InlineKeyboardButton.WithCallbackData(fileNames[i])
                        };
                        ilist.Add(inlineKeyboardButtons.ToArray());
                    }                                      

                    InlineKeyboardMarkup inlineKeyboard = ilist.ToArray();

                    Message sentMessage = await botClient.SendTextMessageAsync(
                            chatId: chatId,
                            text: "Выберите файл для скачивания",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: cancellationToken);
                    return;
                }
                else
                {
                    Message sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "You said:\n" + userAnswer,
                        cancellationToken: cancellationToken);
                }

                //данный код пока почему то не работает
                if (update.Type == UpdateType.CallbackQuery)
                {
                    await HandleCallbackQuery(botClient, update.CallbackQuery!);
                    return;
                }
            }

            async Task HandleCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
            {                
                await using Stream stream = System.IO.File.OpenRead($".\\Сергей\\Документы\\abc2.txt");//строка полного имени файла временная для тестов
                Message message = await botClient.SendDocumentAsync(
                    chatId: callbackQuery.Message!.Chat.Id,
                    document: new InputOnlineFile(content: stream, fileName: "abc2.txt"));//имя файла временное для тестов            

                return;
            }

            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);
                return Task.CompletedTask;
            }

            Console.ReadLine();
        }
    }
}
