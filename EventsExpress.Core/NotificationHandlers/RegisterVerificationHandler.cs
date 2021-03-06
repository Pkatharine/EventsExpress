﻿using System;
using System.Collections.Generic;
using System.Text;
using EventsExpress.Core.DTOs;
using EventsExpress.Core.IServices;
using EventsExpress.Core.Notifications;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using EventsExpress.Core.Infrastructure;

namespace EventsExpress.Core.NotificationHandlers
{
    public class RegisterVerificationHandler :INotificationHandler<RegisterVerificationMessage>
    {
        private readonly IEmailService _sender;
        private readonly IUserService _userService;
        private readonly CacheHelper _cacheHepler;

        public RegisterVerificationHandler(
            IEmailService sender,
            IUserService userSrv,
            CacheHelper cacheHepler
            )
        {
            _sender = sender;
            _userService = userSrv;
            _cacheHepler = cacheHepler;
        }

        public async Task Handle(RegisterVerificationMessage notification, CancellationToken cancellationToken)
        {
            
            Debug.WriteLine("messagehandled");
            var token = Guid.NewGuid().ToString();
            string theEmailLink= "http://localhost:61985/authentication/" + notification.User.Id.ToString() + "/" + token;

            _cacheHepler.Add(new CacheDTO
            {
                UserId = notification.User.Id,
                Token = token
            });

            try
            {
                await _sender.SendEmailAsync(new EmailDTO
                {
                    Subject = "EventExpress registration",
                    RecepientEmail = notification.User.Email,
                    MessageText = $"For confirm your email please follow the <a href='{theEmailLink}'>link</>"
                });

                var x = _cacheHepler.GetValue(notification.User.Id);

                Debug.WriteLine(x.Token);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
