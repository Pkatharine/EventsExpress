﻿using EventsExpress.Core.DTOs;
using EventsExpress.Db.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventsExpress.DTO
{
    public class EventDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateFrom { get; set; }
        public DateTime DateTo { get; set; }
        public IFormFile Photo { get; set; }
        public string PhotoUrl { get; set; }
        public UserPreviewDto User { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public Guid CityId { get; set; }
        public Guid CountryId { get; set; }
                                              
        public IEnumerable<CategoryDto> Categories { get; set; }
        public IEnumerable<UserPreviewDto> Visitors { get; set; }
    }
}
