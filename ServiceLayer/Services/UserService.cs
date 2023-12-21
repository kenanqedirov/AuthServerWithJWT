﻿using CoreLayer.Dtos;
using CoreLayer.Model;
using CoreLayer.Services;
using Microsoft.AspNetCore.Identity;
using SharedLibrary.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> _userManager;

        public UserService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new AppUser
            {
                Email = createUserDto.Email,
                UserName = createUserDto.Username
            };
            var result = await _userManager.CreateAsync(user,createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(a=>a.Description).ToList();
                return Response<UserAppDto>.Fail(new ErrorDto(errors, true),404,true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user),200);
        }

        public async Task<Response<UserAppDto>> GetUserByNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Response<UserAppDto>.Fail("Username not found",404,true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user), 200);
        }
    }
}
