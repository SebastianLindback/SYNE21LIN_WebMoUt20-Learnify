using API.Dto;
using AutoMapper;
using Entity.Specifications;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepository<Course> _repository;
        private readonly StoreContext _context;

        public CoursesController(IGenericRepository<Course> repository, IMapper mapper, StoreContext context)
        {
            _context = context;
            _mapper = mapper;
            _repository = repository;

        }

        [HttpGet]

        public async Task<ActionResult<Pagination<CourseDto>>> GetCourses([FromQuery] CourseParams courseParams)
        {
            var spec = new CoursesWithCategoriesSpecification(courseParams);
            var countSpec = new CoursesFiltersCountSpecification(courseParams);
            var total = await _repository.CountResultAsync(countSpec);
            var courses = await _repository.ListWithSpec(spec);
            var data = _mapper.Map<IReadOnlyList<Course>, IReadOnlyList<CourseDto>>(courses);
            return Ok(new Pagination<CourseDto>(courseParams.PageIndex, courseParams.PageSize, total, data));
        }

        [HttpGet("{id}")]

        public async Task<ActionResult<CourseDto>> GetCourse(Guid id)
        {
            var spec = new CoursesWithCategoriesSpecification(id);
            var course = await _repository.GetEntityWithSpec(spec);
            return _mapper.Map<Course, CourseDto>(course);
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost]

        public async Task<ActionResult<string>> CreateCourse([FromBody] Course course)
        {
            course.Instructor = User.Identity.Name;

            _context.Courses.Add(course);

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return "Course Created Successfully";

            return BadRequest(new ApiResponse(400, "Problem creating Course"));
        }

        [Authorize(Roles = "Instructor")]
        [HttpPost("publish/{courseId}")]

        public async Task<ActionResult<string>> PublishCourse(Guid courseId)
        {

            var course = await _context.Courses.FindAsync(courseId);

            if (course == null) return NotFound(new ApiResponse(404));

            course.Published = true;

            var result = await _context.SaveChangesAsync() > 0;

            if (result) return "Course Published Successfully";

            return BadRequest(new ApiResponse(400, "Problem publishing the Course"));

        }


    }
}