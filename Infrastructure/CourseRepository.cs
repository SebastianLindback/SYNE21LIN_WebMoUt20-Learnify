using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entity;
using Entity.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Infrastructure
{
    public class CourseRepository : ICourseRepository
    {
        private readonly StoreContext _context;
        public CourseRepository(StoreContext context)
        {
            _context = context;
        }

        public async Task<Course> GetCourseByIdAsync(Guid id)
        {
            var course = await _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Learnings)
            .Include(c => c.Requirements)
            .FirstOrDefaultAsync(x => x.Id == id);
            return course!;
        }


        public async Task<IReadOnlyList<Course>> GetCoursesAsync()
        {
            return await _context.Courses
            .Include(c => c.Category)
            .ToListAsync();
        }
    }
}