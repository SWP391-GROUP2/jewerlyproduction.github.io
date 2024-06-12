﻿using Google.Apis.Auth;
using JewelryProduction.DTO.Account;
using JewelryProduction.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace JewelryProduction.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [HttpPost("register/Staff")]
        public async Task<IActionResult> RegisterStaff([FromBody] RegisterStaffDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                {
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email,
                    Name = registerDTO.Name,
                    PhoneNumber = registerDTO.PhoneNumber,
                };

                var result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, registerDTO.Role);
                    if (roleResult.Succeeded)
                    {
                        return Ok(
                                new NewUserDTO
                                {
                                    Email = user.Email,
                                    Token = _tokenService.CreateToken(user)
                                }
                            );
                    }
                    else
                    {
                        return BadRequest("Role Error");
                    }
                }
                else
                {
                    return BadRequest("Create Error");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("register/Customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerDTO registerDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var user = new AppUser
                {
                    Email = registerDTO.Email,
                    UserName = registerDTO.Email,
                    Name = registerDTO.Name,
                    PhoneNumber = registerDTO.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, registerDTO.Password);
                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                    if (roleResult.Succeeded)
                    {
                        return Ok(
                                new NewUserDTO
                                {
                                    Email = user.Email,
                                    Token = _tokenService.CreateToken(user)
                                }
                            );
                    }
                    else
                    {
                        return BadRequest("Role Error");
                    }
                }
                else
                {
                    return BadRequest("Create Error");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
                return Unauthorized("Invalid Email");
            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if(!result.Succeeded) return Unauthorized("Email not found & Invalid Password");
            return Ok(
                new NewUserDTO
                {
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user)
                }
            );
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok("Logout Successful");
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleUserLoginDTO googleLoginDTO)
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(googleLoginDTO.IdToken, new GoogleJsonWebSignature.ValidationSettings());
            if (payload == null)
                return BadRequest("Invalid Id Token");

            var user = await _userManager.FindByEmailAsync(payload.Email);

            if (user == null)
            {
                user = new AppUser
                {
                    Email = payload.Email,
                    UserName = payload.Email,
                    Name = payload.Name,
                    Avatar = payload.Picture,
                };

                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded) return BadRequest("Failed to create user");

                var info = new UserLoginInfo("Google", payload.Subject, "Google");
                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (!loginResult.Succeeded) return BadRequest("Failed to add external login");
            }

            return Ok(new NewUserDTO
            {
                Email = user.Email,
                Token = _tokenService.CreateToken(user)
            });
        }

    }
}