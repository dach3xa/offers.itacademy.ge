﻿using offers.Application.Models;
using offers.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Services.Categories
{
    public interface ICategoryService
    {
        Task<CategoryResponseModel> CreateAsync(Category account, CancellationToken cancellationToken);
        Task<CategoryResponseModel> GetAsync(int id, CancellationToken cancellationToken);
    }
}
