﻿using AutoMapper;
using EventsExpress.Core.DTOs;
using EventsExpress.Core.Infrastructure;
using EventsExpress.Core.IServices;
using EventsExpress.Db.Entities;
using EventsExpress.Db.IRepo;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EventsExpress.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork Db;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _appEnvironment;
        private readonly IPhotoService _photoService;

        public EventService(
            IUnitOfWork unitOfWork, 
            IMapper mapper, 
            IHostingEnvironment hostingEnvironment,
            IPhotoService photoSrv
            )
        {
            Db = unitOfWork;
            _mapper = mapper;
            _photoService = photoSrv;
            _appEnvironment = hostingEnvironment;
        }
       

        public bool Exists(Guid id) => (Db.EventRepository.Get(id) != null);

        public async Task<OperationResult> AddUserToEvent(Guid userId, Guid eventId)
        {
            var ev = Db.EventRepository
               .Filter( filter: e => e.Id == eventId, includeProperties: "Visitors")
               .FirstOrDefault();

            if (ev == null)
            {
                return new OperationResult(false, "Event not found!", "eventId");
            }

            if (ev.Visitors == null)
            {
                ev.Visitors = new List<UserEvent>();
            }

            ev.Visitors.Add(new UserEvent { EventId = eventId, UserId = userId });
            await Db.SaveAsync();

            return new OperationResult(true);
        }

        public async Task<OperationResult> Delete(Guid id)
        {
            if (id == null)
            {
                return new OperationResult(false, "Id field is '0'", "");
            }
            Event ev = Db.EventRepository.Get(id);
            if (ev == null)
            {
                return new OperationResult(false, "Not found", "");
            }

            var result = Db.EventRepository.Delete(ev);
            await Db.SaveAsync();

            if (result == null)
            {
                return new OperationResult(true);
            }
            else {
                return new OperationResult(false, "Error!", "");
            }
        }

        public IEnumerable<EventDTO> UpcomingEvents(int? num)
        {
            var ev = Db.EventRepository.Filter(
                skip: 0,
                take: num,
                filter: e => e.DateTo <= DateTime.UtcNow,
                orderBy: es => es.OrderBy(e => e.DateFrom)
                ).AsEnumerable();
                

            return _mapper.Map<IEnumerable<EventDTO>>(ev);
        }

        public async Task<OperationResult> Create(EventDTO e)
        {
            Event evnt = _mapper.Map<EventDTO, Event>(e);   
            evnt = Db.EventRepository.Insert(evnt);

            // !uncoment when _photoservice will be done
            //if (e.Photo != null)
            //    evnt.PhotoId = _photoservice.AddPhoto(e.Photo, PhotoTypes.EventPhoto);
            if (e.Photo != null)
            {
                string path = "/files/" + e.Photo.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await e.Photo.CopyToAsync(fileStream);
                }

                Photo photo = Db.PhotoRepository.Insert(new Photo { Path = path });
                evnt.Photo = photo;
                Db.EventRepository.Update(evnt);
            }


            List<EventCategory> eventCategories = new List<EventCategory>();
         
            foreach (var item in e.Categories)
            {
                eventCategories.Add(new EventCategory
                {
                    Event = evnt,
                    Category = Db.CategoryRepository.GetByTitle(item)
                });
            }
            evnt.Categories = eventCategories;
            await Db.SaveAsync();
            return new OperationResult(true, "Ok", "");
        }

        public async Task<OperationResult> Edit(EventDTO e)
        {
            var evnt = Db.EventRepository.Get(e.EventId);
            evnt.Title = e.Title;
            evnt.Description = e.Description;
            evnt.DateFrom = e.DateFrom;
            evnt.DateTo = e.DateTo;
            evnt.CityId = e.City.Id;

            if (e.Photo != null)
            {
                string path = "/files/" + e.Photo.FileName;
                using (var fileStream = new FileStream(_appEnvironment.WebRootPath + path, FileMode.Create))
                {
                    await e.Photo.CopyToAsync(fileStream);
                }

                Photo photo = Db.PhotoRepository.Insert(new Photo { Path = path });
                evnt.Photo = photo;
                Db.EventRepository.Update(evnt);
            }
            List<EventCategory> eventCategories = new List<EventCategory>();

            foreach (var item in e.Categories)
            {
                eventCategories.Add(new EventCategory
                {
                    Event = evnt,
                    Category = Db.CategoryRepository.GetByTitle(item)
                });
            }
            evnt.Categories = eventCategories;
            await Db.SaveAsync();
            return new OperationResult(true, "Ok", "");
        }

        public IEnumerable<EventDTO> Events()
        {
            var events = Db.EventRepository.Get().ToList();

            return _mapper.Map<IEnumerable<Event>, IEnumerable<EventDTO>>(events);
        }

        public EventDTO EventById(Guid eventId)
        {
            var evv = Db.EventRepository.Get(eventId);
            return _mapper.Map<EventDTO>(evv);
        }

        public IEnumerable<EventDTO> EventsByUserId(Guid userId)
        {  
            var evv = Db.EventRepository.Filter(filter: e => e.OwnerId == userId);
            return _mapper.Map<IEnumerable<EventDTO>>(evv);

        }

        public EventDTO Details(Guid event_id)
        {
            var ev = Db.EventRepository.Filter(filter: e => e.Id == event_id, includeProperties: "Title,Description,DateFrom,DateTo,City,Photo,EventCategory,Visitors").FirstOrDefault();
            List<string> Categories = new List<string>();
            foreach (var x in Db.CategoryRepository.EventCategories(event_id))
            {
                Categories.Add(x.Name);
            }
            EventDTO eventDTO = _mapper.Map<Event, EventDTO>(ev);
                
            eventDTO.Visitors = ev.Visitors
               .Select(x => x.User.Id)
               .ToList();

            return eventDTO;
        }
    }
}
