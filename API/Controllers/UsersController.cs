using API.Dto;
using Entity.Specifications;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    public class UsersController : BaseController
    {
        private readonly ILogger<UsersController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly TokenService _tokenService;
        private readonly StoreContext _context;
        private readonly IMapper _mapper;

        public UsersController(ILogger<UsersController> logger, UserManager<User> userManager, TokenService tokenService, StoreContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            _tokenService = tokenService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpPost("login")]

        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized(new ApiResponse(401));
            }

            var userBasket = await ExtractBasket(user.UserName);
            var basket = await ExtractBasket(Request.Cookies["clientId"]!);
            var courses = _context.UserCourses.AsQueryable();

            if (basket != null)
            {
                if (userBasket != null) _context.Baskets.Remove(userBasket);
                basket.ClientId = user.UserName;
                Response.Cookies.Delete("clientId");
                await _context.SaveChangesAsync();
            }

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = basket != null ? _mapper.Map<Basket, BasketDto>(basket) : _mapper.Map<Basket, BasketDto>(userBasket!),
                Courses = courses.Where(x => x.UserId == user.Id).Select(u => u.Course).ToList()!
            };
        }

        [HttpPost("register")]

        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            var user = new User { UserName = registerDto.Username, Email = registerDto.Email };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Code, error.Description);
                }
                return ValidationProblem();
            }

            await _userManager.AddToRoleAsync(user, "Student");

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user)
            };
        }

        [Authorize]
        [HttpPost("addRole")]

        public async Task<ActionResult> AddRole()
        {
            var user = await _userManager.FindByNameAsync(User?.Identity?.Name);

            await _userManager.AddToRoleAsync(user, "Instructor");
            return Ok();
        }

        [Authorize]
        [HttpPost("purchaseCourses")]
        public async Task<ActionResult> AddCourses()
        {
            var basket = await ExtractBasket(User?.Identity?.Name!);

            var user = await _userManager.FindByNameAsync(User?.Identity?.Name);

            foreach (BasketItem course in basket.Items)
            {
                var userCourse = new UserCourse
                {
                    Courseid = course.CourseId,
                    UserId = user.Id
                };
                _context.UserCourses.Add(userCourse);
            }

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest(new ApiResponse(400, "Problem adding Course"));

        }

        [Authorize]
        [HttpGet("currentUser")]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.FindByNameAsync(User?.Identity?.Name);

            var basket = await ExtractBasket(User?.Identity?.Name!);

            var courses = _context.UserCourses.AsQueryable();

            return new UserDto
            {
                Email = user.Email,
                Token = await _tokenService.GenerateToken(user),
                Basket = _mapper.Map<Basket, BasketDto>(basket),
                Courses = courses.Where(x => x.UserId == user.Id).Select(u => u.Course).ToList()!
            };
        }

        [Authorize]
        [HttpGet("unpublishedCourses")]

        public List<Course> unpublishedCourses()
        {
            var courses = _context.Courses.Where(x => x.Instructor == User!.Identity!.Name).Where(x => x.Published == false).ToList();

            return courses;
        }

        private async Task<Basket> ExtractBasket(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                Response.Cookies.Delete("clientId");
                return null;
            }
            var basket = await _context.Baskets
                        .Include(b => b.Items)
                        .ThenInclude(i => i.Course)
                        .OrderBy(i => i.Id)
                        .FirstOrDefaultAsync(x => x.ClientId == clientId);
            return basket!;

        }

    }
}