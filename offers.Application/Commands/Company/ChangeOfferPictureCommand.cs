using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.Commands.Company
{
    public record ChangeOfferPictureCommand(int Id, int AccountId, string NewPhotoURL) : IRequest<Unit>
    {

    }
}
