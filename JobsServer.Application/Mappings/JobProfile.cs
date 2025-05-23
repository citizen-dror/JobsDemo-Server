﻿using AutoMapper;
using JobQueueSystem.Core.DTOs;
using JobsServer.Domain.Entities;

namespace JobsServer.Application.Mappings
{
    public class JobProfile : Profile
    {
        public JobProfile()
        {
            CreateMap<Job, JobDto>().ReverseMap();
            CreateMap<CreateJobDto, Job>();
        }
    }
}
