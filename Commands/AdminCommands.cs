using System.Linq;

using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

using MongoDB.Driver;
using MongoDB.Bson;

using Sakura.Uwu.Models;

namespace Sakura.Uwu.GroupManagement.CommandProcessing {
    static class AdminCommands {
        public async static void ListAdmins(TelegramBotClient client, Message message) {
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var adminString = admins
                .Select(
                    (admin) => string.Format(
                            "• <b>{0} {1}{2}</b>\n   @{3}\n   <code>{4}</code>",
                            admin.User.FirstName,
                            admin.User.LastName,
                            (admin.User.IsBot ? "🤖" : ""),
                            admin.User.Username,
                            admin.User.Id
                        )
                )
                .Aggregate(
                    (s1, s2) =>  s1 + "\n\n" + s2
                );
            await client.SendTextMessageAsync(
                message.Chat.Id,
                adminString,
                replyToMessageId: message.MessageId,
                parseMode: ParseMode.Html
            );
        }
        public async static void WarnUser(TelegramBotClient client, Message message, MongoClient dbms) {
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            var contextMessage = message.ReplyToMessage;
            if(contextMessage.From.IsBot) {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "nuu >w< can't warn senpai!",
                    replyToMessageId: message.MessageId
                );

            } else if(admins.Where(admin => admin.User.Id == contextMessage.From.Id).Count() > 0) {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "nuu >w< can't warn admin-sama!",
                    replyToMessageId: message.MessageId
                );                
            } else {
                var database = dbms.GetDatabase("UserRecords");
                var warnsCollection = database.GetCollection<UserWarns>("Warns");
                var filter = new BsonDocument("UserId", contextMessage.From.Id);
                var results = warnsCollection.FindSync(filter).ToList().ToArray();
                if(results.Length == 0) {
                    var document = new UserWarns(contextMessage.From.Id);
                    warnsCollection.InsertOne(document);
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        string.Format(
                            "Warned\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code>\nWarn Count 1",
                            contextMessage.From.FirstName,
                            contextMessage.From.LastName,
                            contextMessage.From.Username,
                            contextMessage.From.Id
                        ),
                        replyToMessageId: message.MessageId,
                        parseMode: ParseMode.Html
                    );
                } else {
                    var warnCount = results[0].WarnCount + 1;
                    var up = new BsonDocument("$set", new BsonDocument("WarnCount", warnCount));
                    warnsCollection.UpdateOne(filter, up);
                    if(warnCount == 3) {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            string.Format(
                                "Warn limit reached! Kicked\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code> uwu",
                                contextMessage.From.FirstName,
                                contextMessage.From.LastName,
                                contextMessage.From.Username,
                                contextMessage.From.Id
                            ),
                            replyToMessageId: message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, contextMessage.From.Id);                                           
                    } else {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            string.Format(
                                "Warned\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code>\nWarn Count {4}, watch out! >w<",
                                contextMessage.From.FirstName,
                                contextMessage.From.LastName,
                                contextMessage.From.Username,
                                contextMessage.From.Id,
                                warnCount
                            ), 
                            replyToMessageId:message.MessageId,
                            parseMode: ParseMode.Html
                        );
                    }
                }
            }
        }
        public async static void ClearWarnsUser(TelegramBotClient client, Message message, MongoClient dbms) {
            var contextMessage = message.ReplyToMessage;
            var database = dbms.GetDatabase("UserRecords");
            var warnsCollection = database.GetCollection<UserWarns>("Warns");
            var filter = new BsonDocument("UserId", contextMessage.From.Id);
            var results = warnsCollection.FindSync(filter).ToList().ToArray();
            if(results.Length != 0) {
                var up = new BsonDocument("$set", new BsonDocument("WarnCount", 0));
                warnsCollection.UpdateOne(filter, up);
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    string.Format(
                        "Warns reset for\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code> :D",
                        contextMessage.From.FirstName,
                        contextMessage.From.LastName,
                        contextMessage.From.Username,
                        contextMessage.From.Id
                    ), 
                    replyToMessageId:message.MessageId,
                    parseMode: ParseMode.Html
                );
            }
        }
        public static async void BanUser(TelegramBotClient client, Message message) {
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            if(message.ReplyToMessage != null) {
                var originMessage = message.ReplyToMessage;
                var originUser = originMessage.From;
                if(admins.Where(admin => admin.User.Id == originUser.Id).Count() > 0) {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        "nuu >w< can't ban admin-sama!",
                        replyToMessageId:message.MessageId
                    );
                } else if(originUser != null) {
                    if(originUser.IsBot) {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            "I'm NOT banning senpai! >w<",
                            replyToMessageId: message.MessageId
                        );
                    } else {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            string.Format(
                                "Banned\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code>~ uwu",
                                originUser.FirstName,
                                originUser.LastName,
                                originUser.Username,
                                originUser.Id
                            ),
                            replyToMessageId:message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, originUser.Id);
                    }
                } else {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        "No User Specified",
                        replyToMessageId: message.MessageId
                    );
                }
            }
        }
        public static async void KickUser(TelegramBotClient client, Message message) {
            var admins = await client.GetChatAdministratorsAsync(message.Chat.Id);
            if(message.ReplyToMessage != null) {
                var originMessage = message.ReplyToMessage;
                var originUser = originMessage.From;
                if(admins.Where(admin => admin.User.Id == originUser.Id).Count() > 0) {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        "nuu >w< can't kick admin-sama!",
                        replyToMessageId:message.MessageId
                    );
                } else if(originUser != null) {
                    if(originUser.IsBot) {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            "I'm NOT kicking senpai! >w<",
                            replyToMessageId: message.MessageId
                        );
                    } else {
                        await client.SendTextMessageAsync(
                            message.Chat.Id,
                            string.Format(
                                "Kicked\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code>, cannot rejoin for 1 minute xd",
                                originUser.FirstName,
                                originUser.LastName,
                                originUser.Username,
                                originUser.Id
                            ),
                            replyToMessageId:message.MessageId,
                            parseMode: ParseMode.Html
                        );
                        await client.KickChatMemberAsync(message.Chat.Id, originUser.Id, System.DateTime.Now.AddMinutes(1));
                    }
                } else {
                    await client.SendTextMessageAsync(
                        message.Chat.Id,
                        "No User Specified",
                        replyToMessageId: message.MessageId
                    );
                }
            }
        }
        public static async void UnbanUser(TelegramBotClient client, Message message) {
            if(message.ReplyToMessage != null) {
                var originMessage = message.ReplyToMessage;
                var originUser = originMessage.From;
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    string.Format(
                        "Unbanned\n<b>{0} {1}</b>\n@{2}\n<code>{3}</code> :D",
                        originUser.FirstName,
                        originUser.LastName,
                        originUser.Username,
                        originUser.Id
                    ),
                    parseMode: ParseMode.Html
                );
                await client.UnbanChatMemberAsync(message.Chat.Id, originUser.Id);
            } else {
                await client.SendTextMessageAsync(
                    message.Chat.Id,
                    "No User Specified",
                    replyToMessageId: message.MessageId
                );
            }
        }
    }
}
