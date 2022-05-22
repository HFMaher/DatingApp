using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext  _context;
        public ITokenService _tokenService { get; }

        public AccountController(DataContext context, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _context=context;
        }

        [HttpPost("register")]        //url/api/account/register, parameters can be sent either using query string or from the body
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)  //dto so we can pass an object from the request body(as in postman)
        {
            if(await UserExists(registerDto.Username)) return BadRequest("Username is taken");

            using var hmac=new HMACSHA512();

            var user=new AppUser
            {
                UserName=registerDto.Username,
                PasswordHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(registerDto.Password)),
                PasswordSalt=hmac.Key
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new UserDto
            {
              Username=user.UserName,
              Token=_tokenService.CreateToken(user)
            };

            
        }

        private async Task<bool> UserExists(string username)
        {
            return await _context.Users.AnyAsync(x=>x.UserName==username.ToLower());
        }

       [HttpPost("login")] 
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user=await _context.Users
            .SingleOrDefaultAsync(x=>x.UserName==loginDto.Username);

            if (user==null) return Unauthorized("Invalid username");

            using var hmac=new HMACSHA512(user.PasswordSalt);

            var computedHash=hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(loginDto.Password));

            for (int i=0;i<computedHash.Length;i++)
            {
                if(computedHash[i] !=user.PasswordHash[i]) return Unauthorized("Invalid password");
            }

            return new UserDto
            {
              Username=user.UserName,
              Token=_tokenService.CreateToken(user)
            };
        }
        
    }
}