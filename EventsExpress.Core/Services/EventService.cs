﻿using AutoMapper;
using EventsExpress.Core.DTOs;
using EventsExpress.Core.Infrastructure;
using EventsExpress.Core.IServices;
using EventsExpress.Db.Entities;
using EventsExpress.Db.IRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using EventsExpress.Core.Notifications;

namespace EventsExpress.Core.Services
{
    public class EventService : IEventService
    {
        private readonly IUnitOfWork Db;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly IMediator _mediator;

        public EventService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IMediator mediator,
            IPhotoService photoSrv
            )
        {
            Db = unitOfWork;
            _mapper = mapper;
            _photoService = photoSrv;
            _mediator = mediator;
        }


        public async Task<OperationResult> AddUserToEvent(Guid userId, Guid eventId)
        {
            var ev = Db.EventRepository.Get("Visitors").FirstOrDefault(e => e.Id == eventId);
            if (ev == null)
            {
                return new OperationResult(false, "Event not found!", "eventId");
            }

            var us = Db.UserRepository.Get(userId);
            if(us == null)
            {                     
                return new OperationResult(false, "User not found!", "userID");
            }

            if (ev.Visitors == null)
            {
                ev.Visitors = new List<UserEvent>();
            }
            ev.Visitors.Add(new UserEvent { EventId = eventId, UserId = userId });
            await Db.SaveAsync();

            return new OperationResult(true);
        }

        public async Task<OperationResult> DeleteUserFromEvent(Guid userId, Guid eventId)
        {
            var ev = Db.EventRepository.Get("Visitors").FirstOrDefault(e => e.Id == eventId);
            if (ev == null)
            {
                return new OperationResult(false, "Event not found!", "eventId");
            }

            var v = ev.Visitors?.FirstOrDefault(x => x.UserId == userId);
            if (v != null)
            {
                (ev.Visitors).Remove(v);
                await Db.SaveAsync();

                return new OperationResult(true);
            }

            return new OperationResult(false, "Visitor not found!", "visitorId");
        }

        public async Task<OperationResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return new OperationResult(false, "Id field is '0'", "");
            }
            var ev = Db.EventRepository.Get(id);
            if (ev == null)
            {
                return new OperationResult(false, "Not found", "");
            }

            var result = Db.EventRepository.Delete(ev);
            await Db.SaveAsync();

            if (result != null)
            {                                
                return new OperationResult(true);
            }                                          
            return new OperationResult(false, "Error!", "");
        }

        public async Task<OperationResult> BlockEvent(Guid eID)
        {
            var uEvent = Db.EventRepository.Get(eID);
            if (uEvent == null)
            {
                return new OperationResult(false, "Invalid event id", "eventId");
            }
            uEvent.IsBlocked = true;

            await Db.SaveAsync();
            await _mediator.Publish(new BlockedEventMessage(uEvent.OwnerId, uEvent.Id));

            return new OperationResult(true);
        }

        public async Task<OperationResult> UnblockEvent(Guid eId)
        {
            var uEvent = Db.EventRepository.Get(eId);
            if (uEvent == null)
            {
                return new OperationResult(false, "Invalid event Id", "eventId");
            }

            uEvent.IsBlocked = false;

            await Db.SaveAsync();
            await _mediator.Publish(new UnblockedEventMessage(uEvent.OwnerId, uEvent.Id));

            return new OperationResult(true);
        }

        public async Task<OperationResult> Create(EventDTO eventDTO)
        {
            if (eventDTO.DateFrom == DateTime.MinValue)
            {
                eventDTO.DateFrom = DateTime.Today;
            }

            if (eventDTO.DateTo == DateTime.MinValue)
            {
                eventDTO.DateTo = DateTime.Today;
            }

            var ev = _mapper.Map<EventDTO, Event>(eventDTO);
            try
            {
                ev.Photo = await _photoService.AddPhoto(eventDTO.Photo);
            }
            catch
            {
                return new OperationResult(false, "Invalid file", "");
            }
                
            var eventCategories = eventDTO.Categories?
                .Select(x => new EventCategory{ Event = ev, CategoryId = x.Id })
                .ToList();
            ev.Categories = eventCategories;
            
            try
            {
                var result = Db.EventRepository.Insert(ev);
                await Db.SaveAsync();

                eventDTO.Id = result.Id;
                await _mediator.Publish(new EventCreatedMessage(eventDTO));
                return new OperationResult(true, "Create new Event", result.Id.ToString());
            }
            catch (Exception ex)
            {
                return new OperationResult(false, ex.Message, "");
            }
        }

        public async Task<OperationResult> Edit(EventDTO e)
        {
            var ev = Db.EventRepository.Get("Photo,Categories.Category").FirstOrDefault(x => x.Id == e.Id);
            ev.Title = e.Title;
            ev.Description = e.Description;
            ev.DateFrom = e.DateFrom;
            ev.DateTo = e.DateTo;
            ev.CityId = e.CityId;

            if (e.Photo != null && ev.Photo != null)
            {
                await _photoService.Delete(ev.Photo.Id);
                try
                {
                    ev.Photo = await _photoService.AddPhoto(e.Photo);
                }
                catch
                {
                    return new OperationResult(false, "Invalid file", "");
                }
            }

            var eventCategories = e.Categories?.Select(x => new EventCategory { Event = ev, CategoryId = x.Id })
                .ToList();

            ev.Categories = eventCategories;

            await Db.SaveAsync();
            return new OperationResult(true, "Edit event", ev.Id.ToString());
        }

        public EventDTO EventById(Guid eventId) =>
            _mapper.Map<EventDTO>(Db.EventRepository
                .Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors.User.Photo")
                .FirstOrDefault(x => x.Id == eventId));
          
        public IEnumerable<EventDTO> Events(EventFilterViewModel model, out int count)
        {
            var events = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors").Where(x=>x.IsBlocked==false);

            events = !string.IsNullOrEmpty(model.KeyWord) ? events.Where(x => x.Title.Contains(model.KeyWord) 
                                                                        || x.Description.Contains(model.KeyWord) 
                                                                        || x.City.Name.Contains(model.KeyWord)
                                                                        || x.City.Country.Name.Contains(model.KeyWord)) : events;
            events = (model.DateFrom != DateTime.MinValue) ? events.Where(x => x.DateFrom >= model.DateFrom) : events.Where(x => x.DateFrom >= DateTime.Today);
            events = (model.DateTo != DateTime.MinValue) ? events.Where(x => x.DateTo <= model.DateTo) : events;

            if(model.Categories != null)
            {
                var categoryIds = model.Categories.Split(",")
                    .Select(x => (Guid.TryParse(x, out Guid item)) ? item : Guid.Empty)
                    .Where(x => x != Guid.Empty)
                    .ToList();

                events = events.Where(x => x.Categories.Any(category => categoryIds.Contains(category.CategoryId)));  
            }

            count = events.Count();

            return _mapper.Map<IEnumerable<EventDTO>>(events.OrderBy(x => x.DateFrom).Skip((model.Page - 1) * model.PageSize).Take(model.PageSize));
        }

        public IEnumerable<EventDTO> EventsForAdmin(EventFilterViewModel model, out int count)
        {
            var events = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors");

            events = !string.IsNullOrEmpty(model.KeyWord) ? events.Where(x => x.Title.Contains(model.KeyWord) 
                                                                        || x.Description.Contains(model.KeyWord)
                                                                        || x.City.Name.Contains(model.KeyWord)
                                                                        || x.City.Country.Name.Contains(model.KeyWord)) : events;
            events = (model.DateFrom != DateTime.MinValue) ? events.Where(x => x.DateFrom >= model.DateFrom) : events;
            events = (model.DateTo != DateTime.MinValue) ? events.Where(x => x.DateTo <= model.DateTo) : events;
            events = (model.Blocked) ? events.Where(x => x.IsBlocked == model.Blocked) : events;
            events = (model.Unblocked) ? events.Where(x => x.IsBlocked == !(model.Unblocked)) : events;

            if (model.Categories != null)
            {
                var categoryIds = model.Categories.Split(",")
                    .Select(x => (Guid.TryParse(x, out Guid item)) ? item : Guid.Empty)
                    .Where(x => x != Guid.Empty)
                    .ToList();

                events = events.Where(x => x.Categories.Any(category => categoryIds.Contains(category.CategoryId)));
            }

            count = events.Count();

            return _mapper.Map<IEnumerable<EventDTO>>(events.OrderBy(x => x.DateFrom).Skip((model.Page - 1) * model.PageSize).Take(model.PageSize));
        }

        public IEnumerable<EventDTO> FutureEventsByUserId(Guid userId ,PaginationViewModel paginationViewModel)
        {
            var ev = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors.User.Photo")
                .Where(e => e.OwnerId == userId && e.DateFrom >= DateTime.Today)
                .OrderBy(e => e.DateFrom)
                .AsEnumerable();


            paginationViewModel.Count = ev.Count();
            return _mapper.Map<IEnumerable<EventDTO>>(ev.Skip((paginationViewModel.Page - 1) * paginationViewModel.PageSize).Take(paginationViewModel.PageSize));
        }

        public IEnumerable<EventDTO> PastEventsByUserId(Guid userId, PaginationViewModel paginationViewModel)
        {
           var ev = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors.User.Photo")
                .Where(e => e.OwnerId == userId && e.DateFrom < DateTime.Today)
                .OrderBy(e => e.DateFrom)
                .AsEnumerable();

            paginationViewModel.Count = ev.Count();
            return _mapper.Map<IEnumerable<EventDTO>>(ev.Skip((paginationViewModel.Page - 1) * paginationViewModel.PageSize).Take(paginationViewModel.PageSize));
        }

        public IEnumerable<EventDTO> VisitedEventsByUserId(Guid userId, PaginationViewModel paginationViewModel)
        {
            var ev = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors.User.Photo")
                .Where(e => e.Visitors.Any(x => x.UserId == userId) && e.DateFrom < DateTime.Today)
                .OrderBy(e => e.DateFrom)
                .AsEnumerable();

            paginationViewModel.Count = ev.Count();
            return _mapper.Map<IEnumerable<EventDTO>>(ev.Skip((paginationViewModel.Page - 1) * paginationViewModel.PageSize).Take(paginationViewModel.PageSize));
        }

        public IEnumerable<EventDTO> EventsToGoByUserId(Guid userId, PaginationViewModel paginationViewModel)
        {
            var ev = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors.User.Photo")
               .Where(e => e.Visitors.Where(x => x.UserId == userId).FirstOrDefault().UserId == userId).Where(x => x.DateTo < DateTime.UtcNow).AsEnumerable();

            paginationViewModel.Count = ev.Count();
            return _mapper.Map<IEnumerable<EventDTO>>(ev.Skip((paginationViewModel.Page - 1) * paginationViewModel.PageSize).Take(paginationViewModel.PageSize));
        }

        public IEnumerable<EventDTO> GetEvents(List<Guid> eventIds, PaginationViewModel paginationViewModel)
        {
            var events = Db.EventRepository.Get("Photo,Owner.Photo,City.Country,Categories.Category,Visitors")
                .Where(x => eventIds.Contains(x.Id));
            paginationViewModel.Count = events.Count();
            return _mapper.Map<IEnumerable<EventDTO>>(events.Skip((paginationViewModel.Page - 1) * paginationViewModel.PageSize).Take(paginationViewModel.PageSize));
        }

        public async Task<OperationResult> SetRate(Guid userId, Guid eventId, byte rate)
        {
            try
            {
                var ev = Db.EventRepository.Get("Rates").FirstOrDefault(e => e.Id == eventId);
                ev.Rates = ev.Rates ?? new List<Rate>();
                
                var currentRate = ev.Rates.FirstOrDefault(x => x.UserFromId == userId && x.EventId == eventId);
                
                if (currentRate == null)
                {
                    ev.Rates.Add(new Rate { EventId = eventId, UserFromId = userId, Score = rate });
                }
                else
                {
                    currentRate.Score = rate;
                }
                await Db.SaveAsync();
                return new OperationResult(true);
            }
            catch (Exception e)
            {
                return new OperationResult(false, e.Message, "");
            }
        }

        public byte GetRateFromUser(Guid userId, Guid eventId)
        {
            return Db.RateRepository.Get()
                       .FirstOrDefault(r => r.UserFromId == userId && r.EventId == eventId)
                           ?.Score ?? 0;
        }

        public double GetRate(Guid eventId)
        {
            return Db.RateRepository.Get().Where(r => r.EventId == eventId).Average(r => r.Score);
        }

        public bool UserIsVisitor(Guid userId, Guid eventId) => 
            Db.EventRepository
                .Get("Visitors").FirstOrDefault(e => e.Id == eventId)?.Visitors?.FirstOrDefault(v => v.UserId == userId) != null;

        public bool Exists(Guid id) => (Db.EventRepository.Get(id) != null);
    }

    
}

