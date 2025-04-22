using MediatR;
using offers.Application.Services.Accounts;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Company
{
    public class ChangePictureHandler : IRequestHandler<ChangePictureCommand, Unit>
    {
        private readonly IAccountService _accountService;

        public ChangePictureHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<Unit> Handle(ChangePictureCommand request, CancellationToken cancellationToken)
        {
            await _accountService.ChangePictureAsync(request.AccountId, request.NewPhotoURL, cancellationToken);
            return Unit.Value;
        }
    }
}
