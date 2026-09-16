using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using WebApi2.Models;
using static WebApi2.Controllers.ValuesController;

namespace WebApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly UTeM_Student_Attendance_AppEntities ent;

        private readonly ValuesController _valuesController;

        public HomeController(UTeM_Student_Attendance_AppEntities ent)
        {
            this.ent = ent;

            _valuesController = new ValuesController(ent);
        }

        public class LowAttendanceListInfo
        {
            public string StudentName { get; set; }
            public string StudentId { get; set; }
            public string ClassName { get; set; }
            public double AttendanceRate { get; set; }
        }

        public class DownloadReportInfo
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string SessionType { get; set; }
            public List<LowAttendanceListInfo> lowAttendanceListInfos { get; set; }
        }

        [HttpPost("DownloadCsv_LowAttendance_Current")]
        public IActionResult DownloadCsv_LowAttendance_Current([FromBody] DownloadReportInfo results)
        {
            if (results == null)
                return BadRequest("No data received.");

            // Build CSV in memory
            var sb = new StringBuilder();
            sb.AppendLine($"Subject ID,{results.SubjectId}"); 
            sb.AppendLine($"Subject Name,{results.SubjectName}"); 
            sb.AppendLine($"Session Type,{results.SessionType}");
            sb.AppendLine();
            sb.AppendLine($"Student Name,Matric No,Class Name, Attendance Rate"); // header

            foreach (var student in results.lowAttendanceListInfos)
            {
                sb.AppendLine($"{student.StudentName},{student.StudentId},{student.ClassName},{student.AttendanceRate}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Return CSV file
            return File(bytes, "text/csv", "result.csv");
        }

        public class DownloadScheduleReportInfo
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string SessionType { get; set; }
            public List<int> ClassId { get; set; }
            public int WeekNum { get; set; }
        }

        public class totalAttendancePerClass
        {
            public int ClassId { get; set; }
            public double totalAttendance { get; set; }
        }

        [HttpPost("DownloadCsv_LowAttendance_Schedule")]
        public IActionResult DownloadCsv_LowAttendance_Schedule([FromBody] DownloadScheduleReportInfo info)
        {
            if (info == null)
                return BadRequest("No data received.");

            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);

            //var semesterId = semester.SemesterId;
            //var startDate = semester.Week1Date;
            var endDate = semester.Week1Date.AddDays(((info.WeekNum - 1) * 7) + 6);

            var totalAttendancePerClassList = new List<totalAttendancePerClass>();

            foreach (var classes in info.ClassId)
            {
                int totalAttendance = ent.Qrsessions.Where(s => s.SubjectId == info.SubjectId && s.Status == 1 && s.SessionType == info.SessionType && s.CreatedTime >= semester.Week1Date && s.CreatedTime <= semester.Week15Date
                    && s.QrsessionClasses.Any(qc => qc.ClassId == classes)).Count();

                totalAttendancePerClassList.Add(new totalAttendancePerClass
                {
                    ClassId = classes,
                    totalAttendance = totalAttendance
                });
            }

            var studentList = ent.Enrolments.Where(x => x.SubjectId == info.SubjectId && x.SemesterId == semester.SemesterId && info.ClassId.Contains((int)x.Student.ClassId)).Include(x => x.Student).Include(x => x.Student.Class).OrderBy(x => x.Student.Class.Session).ThenBy(x => x.Student.Class.Group).ThenBy(x => x.Student.Name).ToList();

            var result = new List<LowAttendanceListInfo>();

            //Console.WriteLine(totalAttendance);
            //int attendance = 0;

            foreach (var student in studentList)
            {
                int attendance = ent.Attendances.Where(x => x.SubjectId == info.SubjectId && x.Qrsession.QrsessionClasses.Any(qc => qc.ClassId == student.Student.ClassId) && x.Qrsession.SessionType == info.SessionType && x.MatricNo == student.MatricNo && x.IsValid == 1 && x.IsRegisteredStudent == 1 && x.Date >= semester.Week1Date && x.Date <= endDate).Count();
                //Console.WriteLine(attendance);

                var totalSession = totalAttendancePerClassList.First(x => x.ClassId == student.Student.ClassId).totalAttendance;

                double attendanceRate = ((double)attendance / totalSession) * 100;
                if (attendanceRate > 100)
                {
                    attendanceRate = 100;
                }

                var studentClass = ent.Classes.First(x => x.ClassId == student.Student.ClassId);
                var className = "S" + studentClass.Session + "G" + studentClass.Group;

                if (attendanceRate < 80.00)
                {
                    var studentInfo = new LowAttendanceListInfo
                    {
                        StudentId = student.MatricNo,
                        StudentName = student.Student.Name,
                        ClassName = className,
                        AttendanceRate = Math.Round(attendanceRate, 2)
                    };
                    result.Add(studentInfo);
                }
            }

            // Build CSV in memory
            var sb = new StringBuilder();
            sb.AppendLine($"Subject ID,{info.SubjectId}");
            sb.AppendLine($"Subject Name,{info.SubjectName}");
            sb.AppendLine($"Session Type,{info.SessionType}");
            sb.AppendLine();
            sb.AppendLine($"Student Name,Matric No,Class Name, Attendance Rate"); // header

            foreach (var student in result)
            {
                sb.AppendLine($"{student.StudentName},{student.StudentId},{student.ClassName},{student.AttendanceRate}");
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Return CSV file
            return File(bytes, "text/csv", "result.csv");
        }

        public class WeeklyAttendanceInfo
        {
            public int WeekNum { get; set; }
            public int isValid { get; set; }
        }

        public class FullAttendanceListInfo
        {
            public string StudentName { get; set; }
            public string StudentId { get; set; }
            public string ClassName { get; set; }
            public double AttendanceRate { get; set; }

            public List<WeeklyAttendanceInfo> weeklyAttendanceInfos { get; set; }
        }

        public class DownloadFullReportInfo
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string SessionType { get; set; }
            public List<FullAttendanceListInfo> fullAttendanceListInfos { get; set; }
        }

        [HttpPost("DownloadCsv_FullAttendance_Current")]
        public IActionResult DownloadCsv_FullAttendance_Current([FromBody] DownloadFullReportInfo results)
        {
            if (results == null)
                return BadRequest("No data received.");

            // Build CSV in memory
            var sb = new StringBuilder();
            sb.AppendLine($"Subject ID,{results.SubjectId}");
            sb.AppendLine($"Subject Name,{results.SubjectName}");
            sb.AppendLine($"Session Type,{results.SessionType}");
            sb.AppendLine();
            sb.Append($"Student Name,Matric No,Class Name,Attendance Rate"); // header

            for (int i = 1; i <= 15; i++)
            {
                sb.Append($",Week {i}");
            }
            sb.AppendLine();

            foreach (var student in results.fullAttendanceListInfos)
            {
                sb.Append($"{student.StudentName},{student.StudentId},{student.ClassName},{student.AttendanceRate}");

                foreach (var weeks in student.weeklyAttendanceInfos)
                {
                    if (weeks.isValid == 0)
                    {
                        sb.Append(",0");
                    }
                    else if (weeks.isValid == 1) 
                    {
                        sb.Append(",/");
                    }
                    else if (weeks.isValid == 2)
                    {
                        sb.Append(",R");
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Icon,Description");
            sb.AppendLine("/,Present");
            sb.AppendLine("0,Absent");
            sb.AppendLine("R,Absent with reason");
            sb.AppendLine("(empty),No class");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Return CSV file
            return File(bytes, "text/csv", "result.csv");
        }

        [HttpPost("DownloadCsv_FullAttendance_Schedule")]
        public IActionResult DownloadCsv_FullAttendance_Schedule([FromBody] DownloadScheduleReportInfo info)
        {
            if (info == null)
                return BadRequest("No data received.");

            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);

            //var semesterId = semester.SemesterId;
            var week1 = semester.Week1Date;
            var endDate = week1.AddDays(((info.WeekNum - 1) * 7) + 6);

            var totalAttendancePerClassList = new List<totalAttendancePerClass>();

            foreach (var classes in info.ClassId)
            {
                int totalAttendance = ent.Qrsessions.Where(s => s.SubjectId == info.SubjectId && s.Status == 1 
                    && s.SessionType == info.SessionType && s.CreatedTime >= semester.Week1Date 
                    && s.CreatedTime <= semester.Week15Date
                    && s.QrsessionClasses.Any(qc => qc.ClassId == classes)).Count();

                totalAttendancePerClassList.Add(new totalAttendancePerClass
                {
                    ClassId = classes,
                    totalAttendance = totalAttendance
                });
            }

            var studentList = ent.Enrolments.Where(x => x.SubjectId == info.SubjectId && x.SemesterId == semester.SemesterId && info.ClassId.Contains((int)x.Student.ClassId))
                .Include(x => x.Student).Include(x => x.Student.Class)
                .OrderBy(x => x.Student.Class.Session).ThenBy(x => x.Student.Class.Group).ThenBy(x => x.Student.Name).ToList();

            var result = new List<FullAttendanceListInfo>();

            //Console.WriteLine(totalAttendance);
            //int attendance = 0;

            foreach (var student in studentList)
            {
                int attendance = ent.Attendances.Where(x => x.SubjectId == info.SubjectId 
                    && x.Qrsession.QrsessionClasses.Any(qc => qc.ClassId == student.Student.ClassId) 
                    && x.Qrsession.SessionType == info.SessionType && x.MatricNo == student.MatricNo 
                    && x.IsValid == 1 && x.Date >= semester.Week1Date && x.Date <= endDate).Count();
                //Console.WriteLine(attendance);

                var totalSession = totalAttendancePerClassList.First(x => x.ClassId == student.Student.ClassId)
                    .totalAttendance;

                double attendanceRate = ((double)attendance / totalSession) * 100;
                if (attendanceRate > 100)
                {
                    attendanceRate = 100;
                }

                var studentClass = ent.Classes.First(x => x.ClassId == student.Student.ClassId);
                var studentClassId = studentClass.ClassId;
                var className = "S" + studentClass.Session + "G" + studentClass.Group;

                var weeklyAttendanceInfo = new List<WeeklyAttendanceInfo>();

                for (int week = 1; week <= info.WeekNum; week++)
                {
                    DateTime weeklyStartDate = week1.AddDays((week - 1) * 7);
                    DateTime weeklyEndDate = weeklyStartDate.AddDays(6);

                    var totalAttendancePerWeek = (from s in ent.Qrsessions
                                                  join sc in ent.QrsessionClasses
                                                      on s.QrsessionId equals sc.QrsessionId
                                                  where s.SubjectId == info.SubjectId && s.Status == 1 
                                                    && s.SessionType == info.SessionType && sc.ClassId == studentClassId 
                                                    && s.CreatedTime >= weeklyStartDate && s.CreatedTime <= weeklyEndDate
                                                  select s.QrsessionId).ToList();

                    var attended = ent.Attendances.Where(a => a.SubjectId == info.SubjectId 
                        && a.MatricNo == student.MatricNo && a.Qrsession.SessionType == info.SessionType 
                        && a.IsValid == 1 && a.IsRegisteredStudent == 1 && a.Date >= weeklyStartDate 
                        && a.Date <= weeklyEndDate)
                        .Select(x => new
                        {
                            x.Status
                        }).ToList();

                    int isValid;

                    if (!totalAttendancePerWeek.Any())
                    {
                        isValid = 3; // No class
                    }
                    else if (!attended.Any()) // 2️. Session exists but student did NOT attend
                    {
                        isValid = 0; // Absent
                    }
                    else
                    {
                        if (attended.Any(x => x.Status == "Present"))
                        {
                            isValid = 1;
                        }
                        else
                        {
                            isValid = 2; // Absent with reason / other
                        }
                    }

                    var weekAttendanceInfo = new WeeklyAttendanceInfo
                    {
                        WeekNum = week,
                        isValid = isValid
                    };

                    weeklyAttendanceInfo.Add(weekAttendanceInfo);
                }
                var studentInfo = new FullAttendanceListInfo
                {
                    StudentId = student.MatricNo,
                    StudentName = student.Student.Name,
                    ClassName = className,
                    AttendanceRate = Math.Round(attendanceRate, 2),
                    weeklyAttendanceInfos = weeklyAttendanceInfo
                };

                result.Add(studentInfo);
            }

            // Build CSV in memory
            var sb = new StringBuilder();
            sb.AppendLine($"Subject ID,{info.SubjectId}");
            sb.AppendLine($"Subject Name,{info.SubjectName}");
            sb.AppendLine($"Session Type,{info.SessionType}");
            sb.AppendLine();
            sb.Append($"Student Name,Matric No,Class Name,Attendance Rate"); // header

            for (int i = 1; i <= info.WeekNum; i++)
            {
                sb.Append($",Week {i}");
            }
            sb.AppendLine();

            foreach (var student in result)
            {
                sb.Append($"{student.StudentName},{student.StudentId},{student.ClassName},{student.AttendanceRate}");

                foreach (var weeks in student.weeklyAttendanceInfos)
                {
                    if (weeks.isValid == 0)
                    {
                        sb.Append(",0");
                    }
                    else if (weeks.isValid == 1)
                    {
                        sb.Append(",/");
                    }
                    else if (weeks.isValid == 2)
                    {
                        sb.Append(",R");
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                }
                sb.AppendLine();
            }

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("Icon,Description");
            sb.AppendLine("/,Present");
            sb.AppendLine("0,Absent");
            sb.AppendLine("R,Absent with reason");
            sb.AppendLine("(empty),No class");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());

            // Return CSV file
            return File(bytes, "text/csv", "result.csv");
        }

        public class StudentCheckResult
        {
            public int Counter { get; set; }
            public string StudentId { get; set; }
            public string StudentName { get; set; }
            public string ClassName { get; set; }
            public bool IsExist { get; set; }
        }


        [HttpPost("UploadCSVFile")]
        public async Task<IActionResult> UploadCSVFile(IFormFile file)
        {
            // Check if file is attached
            if (file == null || file.Length == 0)
                return BadRequest("No CSV file uploaded.");

            if (!file.FileName.EndsWith(".csv"))
                return BadRequest("Only CSV files are allowed.");

            using var reader = new StreamReader(file.OpenReadStream());

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
                return BadRequest("CSV header is missing.");

            var headerColumns = headerLine.Split(',');

            if (headerColumns.Length != 3 || headerColumns[0].Trim() != "StudentId" 
                || headerColumns[1].Trim() != "StudentName" || headerColumns[2].Trim() != "Class")
            {
                return BadRequest("CSV format invalid. CSV header must be exactly: " +
                    "'StudentId', 'StudentName', 'Class'");
            }

            var result = new List<StudentCheckResult>();

            int counter = 1;

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var columns = line.Split(',');

                if (columns.Length != 3)
                {
                    return BadRequest($"Invalid CSV format at row {counter}. Expected 3 columns.");
                }

                var studentId = columns[0].Trim();

                if (!string.IsNullOrEmpty(studentId))
                {
                    var student = ent.Students.Include(x => x.Class).FirstOrDefault(x => x.MatricNo == studentId);

                    if (student == null)
                    {
                        var studentName = columns[1].Trim();
                        var className = columns[2].Trim();
                        result.Add(new StudentCheckResult
                        {
                            Counter = counter,
                            StudentId = studentId,
                            StudentName = studentName,
                            ClassName = className,
                            IsExist = false
                        });
                    }
                    else
                    {
                        result.Add(new StudentCheckResult
                        {
                            Counter = counter,
                            StudentId = studentId,
                            StudentName = student.Name,
                            ClassName = "S" + student.Class.Session + "G" + student.Class.Group,
                            IsExist = true
                        });
                    }                    
                }

                counter++;
            }

            return Ok(result);
        }

        [HttpPost("UploadPDFFile")]
        public async Task<IActionResult> UploadPDFFile(IFormFile file)
        {
            // Check if file is attached
            if (file == null || file.Length == 0)
                return BadRequest("No PDF file uploaded.");

            if (!file.FileName.EndsWith(".pdf"))
                return BadRequest("Only PDF files are allowed.");

            var uploadFolder = @"C:\CCK\UTeM\Sem 5\FYP\Uploaded Student Absent Proof";

            // Create folder if not exists
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            // Extract original file name without extension
            var originalName = Path.GetFileNameWithoutExtension(file.FileName);
            var extension = Path.GetExtension(file.FileName);

            // Append a short GUID (first 8 chars)
            var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);

            var safeFileName = $"{originalName}_{shortGuid}{extension}";
            var filePath = Path.Combine(uploadFolder, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            bool fileSaved = System.IO.File.Exists(filePath);

            return Ok(new
            {
                Status = fileSaved ? "success" : "failed",
                Message = fileSaved ? "File saved successfully" : "File save failed"
            });

        }
    }
}
