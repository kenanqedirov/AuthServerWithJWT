﻿using CoreLayer.Dtos;
using CoreLayer.Model;
using CoreLayer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly UserManager<UserApp> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserService(UserManager<UserApp> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<Response<UserAppDto>> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = new UserApp
            {
                Email = createUserDto.Email,
                UserName = createUserDto.Username,
            };
            var result = await _userManager.CreateAsync(user,createUserDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(a=>a.Description).ToList();
                return Response<UserAppDto>.Fail(new ErrorDto(errors, true),404,true);
            }
            return Response<UserAppDto>.Success(ObjectMapper.Mapper.Map<UserAppDto>(user),200);
        }

        public async Task<Response<NoContentResult>> CreateUserRoles(string userName )
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
                await _roleManager.CreateAsync(new IdentityRole { Name = "Manager" });
            }           
            var user = await _userManager.FindByNameAsync(userName);

            await _userManager.AddToRoleAsync(user, "Admin");
            await _userManager.AddToRoleAsync(user, "Manager");

            return Response<NoContentResult>.Success(StatusCodes.Status201Created);
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
