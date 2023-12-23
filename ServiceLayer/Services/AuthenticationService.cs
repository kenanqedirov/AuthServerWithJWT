using CoreLayer.Configuration;
using CoreLayer.Dtos;
using CoreLayer.Model;
using CoreLayer.Repositories;
using CoreLayer.Services;
using CoreLayer.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly List<Client> _clients;
        private readonly ITokenService _tokenService;
        private readonly UserManager<UserApp> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<UserRefreshToken> _userRefershTokenService;
        private IOptions<List<Client>> options;

        public AuthenticationService(IOptions<List<Client>> options)
        {
            this.options = options;
        }

        public AuthenticationService(IOptions<List<Client>> clients, ITokenService tokenService,
            UserManager<UserApp> userManager,
            IUnitOfWork unitOfWork,
            IGenericRepository<UserRefreshToken> userRefershTokenService)
        {
            _clients = clients.Value;
            _tokenService = tokenService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _userRefershTokenService = userRefershTokenService;
        }

        public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
                return Response<TokenDto>.Fail("Email or Password is wrong!", 400, true);
            if(!await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Response<TokenDto>.Fail("Email or Password is wrong!", 400, true);
            }
            var token = _tokenService.CreateToken(user);
            var userRefreshToken = await _userRefershTokenService.Where(a => a.UserId == user.Id).SingleOrDefaultAsync();
            if (userRefreshToken ==null)
            {
                await _userRefershTokenService.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken ,Expiration= token.RefreshTokenExpiration });
            }
            else
            {
                userRefreshToken.Code = token.RefreshToken;
                userRefreshToken.Expiration = token.RefreshTokenExpiration;
            }
            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(token, 200);
        }

        public Response<ClientTokenDto> CreateTokenByClient(ClientLoginDto clientLoginDto)
        {
            var client = _clients.SingleOrDefault(x => x.Id == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);
            if (client == null)
            {
                return Response<ClientTokenDto>.Fail("Client Id or ClientSecret not found",404,true);
            }
            var token = _tokenService.CreateTokenByClient(client);
            return Response<ClientTokenDto>.Success(token,200);
        }

        public async Task<Response<TokenDto>> CreateTokenByRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefershTokenService.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();
            if (existRefreshToken == null)
            {
                return Response<TokenDto>.Fail("Refresh token not found", 404, true);
            }
            var user = await _userManager.FindByIdAsync(existRefreshToken.UserId);
            if (user == null)
            {
                return Response<TokenDto>.Fail("User Id not found", 404, true);
            }
            var tokenDto = _tokenService.CreateToken(user);
            existRefreshToken.Code = tokenDto.RefreshToken;
            existRefreshToken.Expiration = tokenDto.RefreshTokenExpiration;
            await _unitOfWork.CommitAsync();
            return Response<TokenDto>.Success(tokenDto,200);
        }

        public async Task<Response<NoDataDto>> RevokeRefreshToken(string refreshToken)
        {
            var existRefreshToken = await _userRefershTokenService.Where(a => a.Code == refreshToken).SingleOrDefaultAsync();
            if(existRefreshToken == null)
            {
                return Response<NoDataDto>.Fail("Refresh token not found",404, true);
            }
            _userRefershTokenService.Remove(existRefreshToken);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(200);
        }
    }
}
