﻿using Asp.Versioning;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using offers.Application.Helper;
using offers.API.Infrastructure.Auth.JWT;
using offers.API.Infrastructure.Middlewares;
using offers.API.Infrastructure.Swagger.Examples;
using offers.Application.Models.DTO;
using offers.Application.Models.Response;
using offers.Application.Exceptions;
using offers.Application.Exceptions.Category;
using offers.Application.Services.Accounts;
using offers.Application.Services.Categories;
using offers.Domain.Enums;
using offers.Domain.Models;
using Swashbuckle.AspNetCore.Filters;
using offers.Application.Commands.Admin;
using MediatR;
using offers.Application.Queries.Admin;

namespace offers.API.Controllers.V1
{
    [ApiController]
    [Authorize(Roles = nameof(AccountRole.Admin))]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IMediator mediator, ILogger<AdminController> logger)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new category.
        /// </summary>
        /// <param name="categoryDTO">The category data to be created.</param>
        /// <returns>
        /// A 201 Created response with the newly created category,
        /// or an error response indicating why creation failed.
        /// </returns>
        /// <response code="201">Returns the newly created category</response>
        /// <response code="400">Validation failed (CategoryCouldNotValidateException)</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="409">Category already exists (CategoryAlreadyExistsException)</response>
        /// <response code="500">Internal server error during category creation (CategoryCouldNotBeCreatedException)</response>
        [Produces("application/json")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoryResponseModel))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [SwaggerRequestExample(typeof(CategoryDTO), typeof(CategoryDTOMultipleExamples))]
        [HttpPost("Category")]
        public async Task<IActionResult> Post([FromBody] CategoryDTO categoryDTO, CancellationToken cancellation)
        {
            ControllerHelper.ValidateModelState(
            ModelState,
            errors => throw new CategoryCouldNotValidateException("Could not validate the given category", errors));

            _logger.LogInformation("attempt to add a new category {Name}", categoryDTO.Name);

            var category = categoryDTO.Adapt<Category>();
            var categoryResponse = await _mediator.Send(new CreateCategoryCommand(category), cancellation);

            return CreatedAtAction(nameof(GuestController.GetCategoryById), "Guest", new { id = categoryResponse.Id }, categoryResponse);

        }

        /// <summary>
        /// Retrieves all registered users.
        /// </summary>
        /// <returns>
        /// A 200 OK response containing a list of users.
        /// </returns>
        /// <response code="200">Returns a list of all users</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error if something unexpected goes wrong</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AccountResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 10, CancellationToken cancellation = default)
        {
            var users = await _mediator.Send(new GetAllUsersQuery(pageNumber, pageSize), cancellation);

            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user's information by their ID.
        /// </summary>
        /// <param name="id">The ID of the user to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// A 200 OK response with the user's details,
        /// or a 404 Not Found response if the user does not exist.
        /// </returns>
        /// <response code="200">Returns the user's information</response>
        /// <response code="401">Unauthorized - the requester is not authenticated</response>
        /// <response code="404">User not found (UserNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("users/{id}")]
        public async Task<IActionResult> GetUser(int id, CancellationToken cancellationToken)
        {
            var user = await _mediator.Send(new GetUserQuery(id), cancellationToken);
            return Ok(user);
        }

        /// <summary>
        /// Confirms (activates) a company account by its ID.
        /// </summary>
        /// <param name="id">The ID of the company account to confirm.</param>
        /// <returns>
        /// A 204 NoContent response if the confirmation was successful, or an error response if the company cannot be confirmed.
        /// </returns>
        /// <response code="204">Company confirmed successfully</response>
        /// <response code="400">The company is already active (CompanyAlreadyActiveException)</response>
        /// <response code="404">Company account not found (AccountNotFoundException)</response>
        /// <response code="500">Internal server error during confirmation (AccountCouldNotActivateException)</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpPatch("companies/{id}/confirm")]
        public async Task<IActionResult> ConfirmCompany([FromRoute] int id, CancellationToken cancellationToken)
        {
            _logger.LogInformation("attempt to Confirm a company with the ID {id}", id);
            await _mediator.Send(new ConfirmCompanyCommand(id), cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Retrieves all registered companies.
        /// </summary>
        /// <returns>
        /// A 200 OK response containing a list of companies.
        /// </returns>
        /// <response code="200">Returns a list of all companies</response>
        /// <response code="401">Unauthorized - the user is not authenticated</response>
        /// <response code="500">Internal server error if something unexpected goes wrong</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<AccountResponseModel>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("companies")]
        public async Task<IActionResult> GetAllCompanies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellation = default)
        {
            var companies = await _mediator.Send(new GetAllCompaniesQuery(pageNumber,pageSize), cancellation);

            return Ok(companies);
        }

        /// <summary>
        /// Retrieves a specific company's information by its ID.
        /// </summary>
        /// <param name="id">The ID of the company to retrieve.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>
        /// A 200 OK response with the company's details,
        /// or a 404 Not Found response if the company does not exist.
        /// </returns>
        /// <response code="200">Returns the company's information</response>
        /// <response code="401">Unauthorized - the requester is not authenticated</response>
        /// <response code="404">Company not found (CompanyNotFoundException)</response>
        /// <response code="500">Internal server error</response>
        [Produces("application/json")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompanyResponseModel))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiError))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiError))]
        [HttpGet("companies/{id}")]

        public async Task<IActionResult> GetCompany(int id, CancellationToken cancellationToken)
        {
            var company = await _mediator.Send(new GetCompanyQuery(id), cancellationToken);

            return Ok(company);
        }
    }
}
