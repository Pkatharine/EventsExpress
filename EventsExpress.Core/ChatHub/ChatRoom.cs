﻿using EventsExpress.Core.DTOs;
using EventsExpress.Core.IServices;
using EventsExpress.Db.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace EventsExpress.Core.ChatHub
{
    public class ChatRoom : Hub
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IMessageService _messageService;
        private readonly IEventService _eventService;   
        public ChatRoom(
                IAuthService authService,
                IUserService userService,
                IMessageService messageService,
                IEventService eventService
            )
        {
            _authService = authService;
            _userService = userService;
            _messageService = messageService;
            _eventService = eventService;         
        }

        public async Task Send(Guid chatId, string text)
        {         
            var currentUser = _authService.GetCurrentUser(Context.User);
            var res = await _messageService.Send(chatId, currentUser.Id, text);

            var users = _messageService.GetChatUserIds(res.ChatRoomId);

            await Clients.Users(users).SendAsync("ReceiveMessage", res);        
        }

        public async Task Seen(List<Guid> msgIds)
        {

            var res = await _messageService.MsgSeen(msgIds);
                                                                   
            if (res.Successed)
            {
                var users = _messageService.GetChatUserIds(Guid.Parse(res.Property));
                await Clients.Users(users).SendAsync("WasSeen", msgIds);    
            }

        }
        
        public async Task EventWasCreated(Guid eventId)
        {
            var currentUser = _authService.GetCurrentUser(Context.User);
            var res = _eventService.EventById(eventId);
            var users = _userService.GetUsersByCategories(res.Categories).Where(x => x.Id != currentUser.Id).Select(x => x.Id.ToString()).ToList();
                                                                                                           
            await Clients.Users(users).SendAsync("ReceivedNewEvent", res.Id);
        }

    }
}
