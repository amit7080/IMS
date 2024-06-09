using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMS.Data.Dto
{
    public class PaginationDtos 
    {
        /// <summary>
        /// Search
        /// </summary>
        public string? Search { get; set; }
        /// <summary>
        /// StartDate
        /// </summary>
        public string? StartDate { get; set; }
        /// <summary>
        /// EndDate
        /// </summary>
        public string? EndDate { get; set; }
        /// <summary>
        /// IsActive
        /// </summary>
        public bool IsActive { get; set; }
        [Range(0, int.MaxValue)]
        public virtual int SkipCount { get; set; }
        [Range(1, int.MaxValue)]
        public virtual int MaxResultCount { get; set; } = 10;
    }
}
