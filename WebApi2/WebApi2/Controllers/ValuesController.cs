using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using WebApi2.Models;
using static WebApi2.Controllers.ValuesController;

namespace WebApi2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly UTeM_Student_Attendance_AppEntities ent;

        public ValuesController(UTeM_Student_Attendance_AppEntities ent)
        {
            this.ent = ent;
        }

        public int totalWeek = 15;
        public int midTermBreakWeek = 7;

        public class LoginRequest
        {
            public string UserId { get; set; }
            public string Password { get; set; }
        }


        [HttpPost("LoginStudent")]
        public IActionResult LoginStudent([FromBody] LoginRequest loginRequest)
        {
            var user = ent.Students.Where(x => x.MatricNo == loginRequest.UserId && x.Password == loginRequest.Password)
                .Select(x => new
                {
                    x.MatricNo,
                    x.ClassId,
                    Faculty = x.Class.Course.FacultyName,
                    x.Name,
                    x.Email,
                    x.Phone,
                    x.Password,
                    Role = "Student"
                }).FirstOrDefault();
            if (user == null)
                return BadRequest("Invalid credentials");

            return Ok(user);
        }

        [HttpPost("LoginStaff")]
        public IActionResult LoginStaff([FromBody] LoginRequest loginRequest)
        {
            var user = ent.Staff.Where(x => x.StaffId == loginRequest.UserId && x.Password == loginRequest.Password)
                .Select(x => new
                {
                    x.StaffId,
                    x.Name,
                    x.Email,
                    x.Phone,
                    x.Password,
                    Role = x.Role
                }).FirstOrDefault();
            if (user == null)
                return BadRequest("Invalid credentials");

            return Ok(user);
        }

        [HttpGet("TeachingSubjectList")]
        public IActionResult TeachingSubjectList(string staffId)
        {
            var subjectList = ent.Teachings.Where(x => x.StaffId == staffId)
                .Select(x => new
                {
                    x.SubjectId,
                    x.Subject.SubjectName
                }).GroupBy(x => x.SubjectId).Select(g => g.First()).ToList();

            return Ok(subjectList);
        }

        [HttpGet("CurrentActiveSession")]
        public IActionResult CurrentActiveSession(string staffId, string subjectId)
        {
            var activeSession = ent.Qrsessions.Where(x => x.StaffId == staffId && x.SubjectId == subjectId && x.CreatedTime <= DateTime.Now && x.ExpiryTime >= DateTime.Now && x.Status == 1)
                .Select(x => new
                {
                    x.QrsessionId,
                    x.CreatedTime,
                    x.ExpiryTime, 
                    x.SessionType
                }).FirstOrDefault();

            if (activeSession != null)
            {
                var allSessionIdsForDate = ent.Qrsessions.Where(x => x.SubjectId == subjectId && x.CreatedTime == activeSession.CreatedTime)
                    .Select(x => x.QrsessionId).ToList();

                var attendedStudentList = (from a in ent.Attendances
                                           join s in ent.Students
                                               on a.MatricNo equals s.MatricNo
                                           join c in ent.Classes
                                               on s.ClassId equals c.ClassId
                                           where (allSessionIdsForDate.Contains(a.QrsessionId) && a.Status == "Present")
                                           select new
                                           {
                                               StudentName = s.Name.Trim(),
                                               StudentId = s.MatricNo,
                                               ClassName = "S" + c.Session + "G" + c.Group,
                                               AttendanceTime = a.Date.ToShortTimeString(),
                                               Location = a.Location.Trim(),
                                               a.IsValid, 
                                               a.IsRegisteredStudent
                                           }).ToList();
                return Ok(new { activeSession.QrsessionId, CreatedTime = activeSession.CreatedTime.ToShortTimeString(), ExpiryTime = activeSession.ExpiryTime.ToShortTimeString(), activeSession.SessionType, StudentCount = attendedStudentList.Count(), currentAttendedStudentLists = attendedStudentList });
            }
            return BadRequest("No active session found");
        }

        public class StudentAttendanceValidInfo
        {
            public int QrSessionId { get; set; }
            public string MatricNo { get; set; }
            public int IsValid { get; set; }
        }

        [HttpPost("UpdateStudentAttendanceValidility")]
        public IActionResult UpdateStudentAttendanceValidility([FromBody] StudentAttendanceValidInfo studentAttendanceValidInfo)
        {
            var studentAttendance = ent.Attendances.FirstOrDefault(x => x.MatricNo == studentAttendanceValidInfo.MatricNo && x.QrsessionId == studentAttendanceValidInfo.QrSessionId);
            if (studentAttendance == null)
                return NotFound("Attendance record not found");

            if (studentAttendanceValidInfo.IsValid == 1)
            {
                studentAttendance.IsValid = 0;
            }
            else
            {
                studentAttendance.IsValid = 1;
            }

            ent.SaveChanges();
            return Ok("Student attendance validality update successful");
        }

        [HttpGet("TeachSubjectLectureClassList")] 
        public IActionResult TeachSubjectLectureClassList(string staffId, string role, string subjectId)
        {
            var query = ent.Teachings.AsQueryable();

            // 2. Apply filter only if lecturer
            if (role == "Lecturer")
            {
                query = query.Where(x => x.StaffId == staffId);
            }

            var lecturerClassList = query.Where(x => x.SubjectId == subjectId && x.LectureStatus == 1)
                .Select(x => new
                {
                    x.ClassId, 
                    ClassName = "S" + x.Class.Session + "G" + x.Class.Group
                }).ToList();

            return Ok(lecturerClassList);
        }

        [HttpGet("TeachSubjectLabClassList")]
        public IActionResult TeachSubjectLabClassList(string staffId, string role, string subjectId)
        {
            var query = ent.Teachings.AsQueryable();

            // 2. Apply filter only if lecturer
            if (role == "Lecturer")
            {
                query = query.Where(x => x.StaffId == staffId);
            }

            var labClassList = query.Where(x => x.SubjectId == subjectId && x.LabStatus == 1)
                .Select(x => new
                {
                    x.ClassId,
                    ClassName = "S" + x.Class.Session + "G" + x.Class.Group
                }).ToList();

            return Ok(labClassList);
        }

        public class GenerateQRCodeInfo
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string SessionType { get; set; }
            public string ClassName { get; set; }
            public string QRCode { get; set; }
            public DateTime CreatedTime { get; set; }
            public DateTime ExpiryTime { get; set; }
            public int Status { get; set; }

            public List<int> ClassId { get; set; }
            public string StaffId { get; set; }
        }

        [HttpPost("GenerateQrCode")]
        public IActionResult GenerateQrCode([FromBody] GenerateQRCodeInfo generateQRCodeInfo)
        {
            var qrSession = new Qrsession()
            {
                SubjectId = generateQRCodeInfo.SubjectId,
                StaffId = generateQRCodeInfo.StaffId,
                SessionType = generateQRCodeInfo.SessionType,
                Qrcode = generateQRCodeInfo.QRCode,
                CreatedTime = generateQRCodeInfo.CreatedTime,
                ExpiryTime = generateQRCodeInfo.ExpiryTime,
                Status = generateQRCodeInfo.Status
            };

            ent.Qrsessions.Add(qrSession);
            int qrSessionRows = ent.SaveChanges();

            //var studentList = new List<Student>();

            foreach (var classId in generateQRCodeInfo.ClassId)
            {
                //var student = ent.Students.Where(x => x.ClassId == classId).ToList();
                //studentList.AddRange(student);

                var sessionClass = new QrsessionClass
                {
                    QrsessionId = qrSession.QrsessionId,
                    ClassId = classId
                };

                ent.QrsessionClasses.Add(sessionClass);
            }
            int qrSessionClassRows = ent.SaveChanges();

            //foreach (var student in studentList)
            //{
            //    var attendanceRecord = new Attendance
            //    {
            //        SubjectId = generateQRCodeInfo.SubjectId,
            //        QrsessionId = qrSession.QrsessionId,
            //        MatricNo = student.MatricNo,
            //        Date = DateTime.Now,
            //        Status = "Not Present",
            //        Location = "",
            //        IsValid = 0,
            //        IsRegisteredStudent = 1
            //    };

            //    ent.Attendances.Add(attendanceRecord);
            //}
            //var attendancerow = ent.SaveChanges();

            if (qrSessionRows > 0 && qrSessionClassRows > 0)
            {
                return Ok(qrSession.QrsessionId);
            }
            else if (qrSessionRows == 0)
            {
                return BadRequest("QRSession table insert failed.");
            }
            else
            {
                return BadRequest("QRSessionClass table insert failed.");
            }
        }

        [HttpPost("ReplaceQrCode")]
        public IActionResult ReplaceQrCode([FromBody] GenerateQRCodeInfo generateQRCodeInfo, [FromQuery] int qrSessionId)
        {
            var session = ent.Qrsessions.FirstOrDefault(x => x.QrsessionId == qrSessionId);

            if (session == null)
                return NotFound("Record not found");

            session.Status = 0;

            ent.SaveChanges();

            var qrSession = new Qrsession()
            {
                SubjectId = generateQRCodeInfo.SubjectId,
                StaffId = generateQRCodeInfo.StaffId,
                SessionType = generateQRCodeInfo.SessionType,
                Qrcode = generateQRCodeInfo.QRCode,
                CreatedTime = generateQRCodeInfo.CreatedTime,
                ExpiryTime = generateQRCodeInfo.ExpiryTime,
                Status = generateQRCodeInfo.Status
            };

            ent.Qrsessions.Add(qrSession);
            int qrSessionRows = ent.SaveChanges();

            //var studentList = new List<Student>();

            foreach (var classId in generateQRCodeInfo.ClassId)
            {
                //var student = ent.Students.Where(x => x.ClassId == classId).ToList();
                //studentList.AddRange(student);

                var sessionClass = new QrsessionClass
                {
                    QrsessionId = qrSession.QrsessionId,
                    ClassId = classId
                };

                ent.QrsessionClasses.Add(sessionClass);
            }
            int qrSessionClassRows = ent.SaveChanges();


            //var sessionDate = generateQRCodeInfo.CreatedTime;

            //var allSessionIdsForDate = ent.Qrsessions
            //    .Where(x => x.SubjectId == generateQRCodeInfo.SubjectId && x.CreatedTime == sessionDate)
            //    .Select(x => x.QrsessionId)
            //    .ToList();

            //var existingAttendanceMatricNos = ent.Attendances
            //    .Where(a => allSessionIdsForDate.Contains(a.QrsessionId) && a.SubjectId == generateQRCodeInfo.SubjectId)
            //    .Select(a => a.MatricNo).ToList();

            //foreach (var student in studentList)
            //{
            //    // Skip if student already has attendance record for this subject and date
            //    if (existingAttendanceMatricNos.Contains(student.MatricNo))
            //    {
            //        continue;
            //    }

            //    var attendanceRecord = new Attendance
            //    {
            //        SubjectId = generateQRCodeInfo.SubjectId,
            //        QrsessionId = qrSession.QrsessionId,
            //        MatricNo = student.MatricNo,
            //        Date = DateTime.Now,
            //        Status = "Not Present",
            //        Location = "",
            //        IsValid = 0,
            //        IsRegisteredStudent = 1 // Should be 1 since they're registered students
            //    };
            //    ent.Attendances.Add(attendanceRecord);
            //}
            //var attendancerow = ent.SaveChanges();

            if (qrSessionRows > 0 && qrSessionClassRows > 0)
            {
                return Ok(qrSession.QrsessionId);
            }
            else if (qrSessionRows == 0)
            {
                return BadRequest("QRSession table insert failed.");
            }
            else
            {
                return BadRequest("QRSessionClass table insert failed.");
            }
        }

        [HttpGet("RetrieveExistingQrCode")]
        public IActionResult RetrieveExistingQrCode(int qrSessionId)
        {
            //Console.WriteLine("\nQRSession id : " + qrSessionId);
            var classList = ent.QrsessionClasses.Where(x => x.QrsessionId == qrSessionId)
                .Select(x => $"S{x.Class.Session}G{x.Class.Group}").ToList();

            string className = string.Join(", ", classList);
            //string className = "";

            //foreach (var classes in classList)
            //{
            //    className += "S" + classes.Class.Session + "G" + classes.Class.Group;
            //    className += ", ";
            //}

            //className = className.Substring(0, className.Length - 2);

            var qrInfo = ent.Qrsessions.Where(x => x.QrsessionId == qrSessionId)
                .Select(x => new
                {
                    x.SubjectId,
                    x.Subject.SubjectName,
                    x.SessionType,
                    x.Qrcode,
                    x.CreatedTime,
                    x.ExpiryTime,
                    ClassName = className
                }).FirstOrDefault();

            return Ok(qrInfo);
        }

        [HttpGet("ValidateStudentDevice")]
        public IActionResult ValidateStudentDevice(string studentId, string deviceId, string deviceName)
        {
            var studentDevice = ent.Devices.FirstOrDefault(x => x.MatricNo == studentId);

            var deviceOwner = ent.Devices.FirstOrDefault(x => x.AndroidId == deviceId);

            DateTime now = DateTime.Now;

            if (studentDevice != null) // the student not the first time scan qr 
            {
                if (studentDevice.AndroidId == deviceId) // Same device - just update timestamp (normal case)
                {
                    studentDevice.LastUsed = now;
                    ent.SaveChanges();
                    return Ok(new { Status = "Valid" });
                }
                else // the student using other device
                {
                    if (deviceOwner != null) // Check if the device is being used by someone else
                    {
                        TimeSpan timeSinceLastUse = now - deviceOwner.LastUsed;

                        if (timeSinceLastUse.TotalHours < 2)
                        {
                            return Ok(new { Status = "Invalid", Message = "This device was recently used by " 
                                + $"another student. Please wait {(int)(120 - timeSinceLastUse.TotalMinutes)}" 
                                + " minutes or use your registered device." });
                        }
                    }
                    // if the device is never being use, 
                    // Allow device change (update the student's device record)
                    studentDevice.AndroidId = deviceId;
                    studentDevice.DeviceName = deviceName;
                    studentDevice.LastUsed = now;
                    ent.SaveChanges();

                    return Ok(new { Status = "Valid", Message = "Device changed." });
                }
            }
            else // the student never scan qr before 
            {                
                if (deviceOwner != null) // Check if the device is already registered to someone else
                {
                    TimeSpan timeSinceLastUse = now - deviceOwner.LastUsed;

                    if (timeSinceLastUse.TotalHours < 2)
                    {
                        return Ok(new { Status = "Invalid", Message = $"This device is registered to another student and was used recently. Please wait {(int)(120 - timeSinceLastUse.TotalMinutes)} minutes or use your registered device." });
                    }

                    // Device hasn't been used in 2+ hours, allow new student to check in
                    deviceOwner.MatricNo = studentId;
                    deviceOwner.DeviceName = deviceName;
                    deviceOwner.LastUsed = now;
                    ent.SaveChanges();

                    return Ok(new { Status = "Valid" });
                }

                // Brand new student with brand new device
                var newDevice = new Device
                {
                    MatricNo = studentId,
                    DeviceName = deviceName,
                    AndroidId = deviceId,
                    LastUsed = now
                };
                ent.Devices.Add(newDevice);
                ent.SaveChanges();

                return Ok(new { Status = "Valid" });
            }
        }

        public class AddAttendanceRecord
        {
            public string? SubjectId { get; set; } 
            public string MatricNo { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
            public string Location { get; set; }
            public int IsValid { get; set; }
            public int IsRegisteredStudent { get; set; }

            public string? QRCode { get; set; }
            public int ClassId { get; set; }

            public int QRSessionID { get; set; }
        }

        [HttpPost("AddAttendance")]
        public IActionResult AddAttendance([FromBody] AddAttendanceRecord addAttendanceRecord)
        {
            if (addAttendanceRecord.QRSessionID == 0)
            {
                var qrSessionId = ent.Qrsessions.Where(x => x.Qrcode == addAttendanceRecord.QRCode).FirstOrDefault();

                if (qrSessionId == null)
                {
                    return Ok(new { Status = "failed", Message = "Invalid QR Code." });
                }

                if (qrSessionId.CreatedTime > DateTime.Now || qrSessionId.ExpiryTime < DateTime.Now)
                {
                    return Ok(new { Status = "failed", Message = "The session is expired." });
                }

                addAttendanceRecord.QRSessionID = qrSessionId.QrsessionId;
                addAttendanceRecord.SubjectId = qrSessionId.SubjectId;
            }

            //Console.WriteLine($"Checking - QRSessionID: {addAttendanceRecord.QRSessionID}, ClassId: {addAttendanceRecord.ClassId}");

            var registeredStudent = ent.QrsessionClasses.FirstOrDefault(x => x.QrsessionId == addAttendanceRecord.QRSessionID && x.ClassId == addAttendanceRecord.ClassId);
            var registeredStudentInEnrollment = ent.Enrolments.FirstOrDefault(x => x.SubjectId == addAttendanceRecord.SubjectId && x.MatricNo == addAttendanceRecord.MatricNo);

            //Console.WriteLine($"Found: {registeredStudent != null}");

            if (registeredStudent == null || registeredStudentInEnrollment == null)
            {
                addAttendanceRecord.IsRegisteredStudent = 0;
            }
            else
            {
                addAttendanceRecord.IsRegisteredStudent = 1;
            }

            var sessionDate = ent.Qrsessions.Where(x => x.QrsessionId == addAttendanceRecord.QRSessionID).Select(x => x.CreatedTime).First();

            var allSessionIdsForDate = ent.Qrsessions.Where(x => x.SubjectId == addAttendanceRecord.SubjectId && x.CreatedTime == sessionDate)
                .Select(x => x.QrsessionId).ToList();

            var existingAttendance = ent.Attendances.FirstOrDefault(a => allSessionIdsForDate.Contains(a.QrsessionId) && a.MatricNo == addAttendanceRecord.MatricNo);

            if (existingAttendance != null)
            {
                return Ok(new { Status = "failed", Message = "Attendance already recorded." });
            }

            var attendance = new Attendance
            {
                SubjectId = addAttendanceRecord.SubjectId,
                QrsessionId = (int)addAttendanceRecord.QRSessionID,
                MatricNo = addAttendanceRecord.MatricNo,
                Date = addAttendanceRecord.Date,
                Status = addAttendanceRecord.Status,
                Location = addAttendanceRecord.Location,
                IsValid = addAttendanceRecord.IsValid,
                IsRegisteredStudent = addAttendanceRecord.IsRegisteredStudent
            };

            ent.Attendances.Add(attendance);
            var attendanceRow = ent.SaveChanges();

            if (attendanceRow > 0)
            {
                return Ok(new { Status = "success", Message = "Attendance recorded successfully." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to insert to database." });
            }
        }

        [HttpGet("RetrieveStudentName")]
        public IActionResult RetrieveStudentName(string studentId)
        {
            var student = ent.Students.FirstOrDefault(x => x.MatricNo == studentId);

            if (student == null)
                return Ok(new { Status = "failed", Message = "Student id not found" }); 

            return Ok(new { Status = "success", student.Name, student.ClassId });
        }

        [HttpGet("RetrieveStudentTodayAttendance")]
        public IActionResult RetrieveStudentTodayAttendance(string studentId)
        {
            var attendence = ent.Attendances.Where(x => x.MatricNo == studentId && x.Date >= DateTime.Today && x.Date <= DateTime.Now && x.Status == "Present")
                .Select(x => new
                {
                    x.SubjectId, 
                    x.Subject.SubjectName, 
                    x.IsValid, 
                    x.Qrsession.SessionType, 
                    StartTime = Convert.ToInt32(x.Qrsession.CreatedTime.ToString("HH")),
                    EndTime = Convert.ToInt32(x.Qrsession.ExpiryTime.ToString("HH"))
                }).ToList();

            return Ok(attendence);
        }

        public class StudentAttendanceGraph
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public double attendanceRate { get; set; }
            public int SemesterId { get; set; }
        }

        [HttpGet("RetrieveStudentAttendanceGraph")]
        public IActionResult RetrieveStudentAttendanceGraph(string studentId, int classId)
        {
            var confirm = ent.Students.FirstOrDefault(x => x.MatricNo == studentId && x.ClassId == classId);
            
            if (confirm == null)
            {
                return BadRequest("Student ID and class ID is not match");
            }

            var subjectList = ent.Enrolments.Where(x => x.MatricNo == studentId)
                .Select(x => new
                {
                    x.SubjectId,
                    x.Subject.SubjectName,
                    x.SemesterId
                }).ToList();

            if (subjectList.Count == 0)
            {
                return null;
            }

            var result = new List<StudentAttendanceGraph>();

            //int totalAttendance = 0;
            //int attendance = 0;

            var semStartDate = ent.Semesters.First(x => x.SemesterId == subjectList.First().SemesterId).Week1Date;
            var semEndDate = ent.Semesters.First(x => x.SemesterId == subjectList.First().SemesterId).Week15Date;

            foreach (var subject in subjectList)
            {
                var totalAttendance = (from s in ent.Qrsessions
                                       join sc in ent.QrsessionClasses
                                           on s.QrsessionId equals sc.QrsessionId
                                       where s.SubjectId == subject.SubjectId && s.Status == 1 && sc.ClassId == classId && s.CreatedTime >= semStartDate && s.CreatedTime <= semEndDate
                                       select s).Count();
                //Console.WriteLine(totalAttendance);

                var attendance = ent.Attendances.Where(x => x.SubjectId == subject.SubjectId && x.MatricNo == studentId && x.IsValid == 1 && x.IsRegisteredStudent == 1 && x.Date >= semStartDate && x.Date <= semEndDate).ToList().Count();
                //Console.WriteLine(attendance);

                double attendanceRate = ((double)attendance / totalAttendance) * 100;
                if (totalAttendance == 0)
                {
                    attendanceRate = 0;
                }
                if (attendanceRate > 100)
                {
                    attendanceRate = 100;
                }

                // Handle division by zero
                if (totalAttendance == 0)
                {
                    var studentAttendanceGraph = new StudentAttendanceGraph
                    {
                        SubjectId = subject.SubjectId,
                        SubjectName = subject.SubjectName,
                        attendanceRate = 0,
                        SemesterId = subject.SemesterId
                    };
                    result.Add(studentAttendanceGraph);
                    continue;
                }

                var studentAttendanceGraph2 = new StudentAttendanceGraph
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    attendanceRate = Math.Round(attendanceRate, 2),
                    SemesterId = subject.SemesterId
                };
                result.Add(studentAttendanceGraph2);
            }

            return Ok(result);
        }

        public class StudentAttendanceDetail
        {
            public int WeekNum { get; set; }
            public string LectureLabel { get; set; }
            public string LabLabel { get; set; }
            public int isValidLecture { get; set; }
            public int isValidLab { get; set; }
        }

        [HttpGet("RetrieveStudentAttendanceDetail")]
        public IActionResult RetrieveStudentAttendanceDetail(string studentId, int classId, string subjectId, int semesterId)
        {
            var confirm = ent.Students.FirstOrDefault(x => x.MatricNo == studentId && x.ClassId == classId);

            if (confirm == null)
            {
                return BadRequest("Student ID and class ID is not match");
            }

            var semester = ent.Semesters.First(s => s.SemesterId == semesterId);
            if (semester == null)
                return BadRequest("Invalid semester ID");

            DateTime week1 = semester.Week1Date;

            var result = new List<StudentAttendanceDetail>();

            for (int week = 1; week <= totalWeek; week++)
            {
                DateTime startDate = week1.AddDays((week - 1) * 7);
                DateTime endDate = startDate.AddDays(6);

                var totalAttendance = (from s in ent.Qrsessions
                                       join sc in ent.QrsessionClasses
                                           on s.QrsessionId equals sc.QrsessionId
                                       where s.SubjectId == subjectId && s.Status == 1 && sc.ClassId == classId && s.CreatedTime >= startDate && s.CreatedTime <= endDate
                                       select new { 
                                           s.QrsessionId, 
                                           s.SessionType }).ToList();

                var attended = ent.Attendances.Where(a => a.SubjectId == subjectId && a.MatricNo == studentId && a.IsValid == 1 && a.IsRegisteredStudent == 1 && a.Date >= startDate && a.Date <= endDate)
                    .Select(x => new
                    {
                        x.QrsessionId,
                        x.Qrsession.SessionType,
                        x.Date,
                        x.Status,
                        x.Location,
                        x.IsValid
                    }).ToList();

                var notAttendedSessions = totalAttendance.Select(x => x.QrsessionId).Except(attended.Select(x => x.QrsessionId)).ToList();

                var lectureLabel = "";
                var labLabel = "";
                var isValidLecture = 3;
                var isValidLab = 3;

                if (notAttendedSessions.Any()) // If missing attendance exists
                {
                    foreach (var sessionId in notAttendedSessions)
                    {
                        var sessionDetails = ent.Qrsessions.First(x => x.QrsessionId == sessionId);

                        if (sessionDetails.SessionType == "Lecture")
                        {
                            lectureLabel = "";
                            isValidLecture = 0;
                        }
                        else
                        {
                            labLabel = "";
                            isValidLab = 0;
                        }
                    }
                }                

                if (attended != null)
                {
                    foreach (var record in attended)
                    {
                        if (record.SessionType == "Lecture")
                        {
                            if (record.Status == "Present")
                            {
                                lectureLabel += "Check-in Date: \n" + record.Date.ToString("dd/MM/yyyy") + "\nCheck-in Time: \n" + record.Date.ToString("HH.mm tt\n");
                                isValidLecture = record.IsValid;
                            }
                            else
                            {
                                lectureLabel = record.Location;
                                isValidLecture = 2;
                            }
                        }
                        else
                        {
                            if (record.Status == "Present")
                            {
                                labLabel += "Check-in Date: \n" + record.Date.ToString("dd/MM/yyyy") + "\nCheck-in Time: \n" + record.Date.ToString("HH.mm tt\n");
                                isValidLab = record.IsValid;
                            }
                            else
                            {
                                labLabel = record.Location;
                                isValidLab = 2;
                            }
                        }
                    }
                }

                result.Add(new StudentAttendanceDetail
                {
                    WeekNum = week,
                    LectureLabel = lectureLabel.Trim(),
                    LabLabel = labLabel.Trim(),
                    isValidLecture = isValidLecture,
                    isValidLab = isValidLab
                });
            }

            return Ok(result);
        }

        public class Report_SubjectList
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public int PeopleNum { get; set; }
        }

        [HttpGet("RetrieveReportSubjectList")]
        public IActionResult RetrieveReportSubjectList(string staffId, string role)
        {
            var query = ent.Teachings.AsQueryable();

            // 2. Apply filter only if lecturer
            if (role == "Lecturer")
            {
                query = query.Where(x => x.StaffId == staffId);
            }

            query = query.Where(x => x.Subject.Semester.Week1Date <= DateTime.Now && x.Subject.Semester.Week15Date >= DateTime.Now);

            var subjectList = query.Select(x => new
                {
                    x.SubjectId,
                    x.Subject.SubjectName,
                    x.Subject.SemesterId,
                    x.Subject.Semester.Week1Date.Year
                }).GroupBy(x => x.SubjectId).Select(g => g.First()).ToList();

            var result = new List<Report_SubjectList>();

            foreach (var subject in subjectList)
            {
                var peopleNum = ent.Enrolments.Where(x => x.SubjectId == subject.SubjectId && x.SemesterId == subject.SemesterId && x.Year == subject.Year).ToList().Count();

                result.Add(new Report_SubjectList
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    PeopleNum = peopleNum
                });
            }

            return Ok(result);
        }

        public class TeachSubjectClassList
        {
            public int ClassId { get; set; }
            public string ClassName { get; set; }

            public bool IsSelected { get; set; }
        }

        public class SubjectInfo
        {
            public string SubjectId { get; set; }
            public bool isLecture { get; set; }
            public List<TeachSubjectClassList> ClassInfo { get; set; }
        }

        public class totalAttendancePerClass
        {
            public int ClassId { get; set; }
            public double totalAttendance {  get; set; }
        }

        public class LowAttendanceListInfo
        {
            public string StudentName { get; set; }
            public string StudentId { get; set; }
            public string ClassName { get; set; }
            public double AttendanceRate { get; set; }
        }

        [HttpPost("RetrieveLowAttendanceList")]
        public IActionResult RetrieveLowAttendanceList([FromBody] SubjectInfo subjectInfo)
        {
            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);

            var classIdList = subjectInfo.ClassInfo.Select(x => x.ClassId).ToList();

            var totalAttendancePerClassList = new List<totalAttendancePerClass>();

            foreach (var classes in classIdList)
            {
                int totalAttendance = ent.Qrsessions.Where(s => s.SubjectId == subjectInfo.SubjectId && s.Status == 1 && s.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab") && s.CreatedTime >= semester.Week1Date && s.CreatedTime <= semester.Week15Date
                    && s.QrsessionClasses.Any(qc => qc.ClassId == classes)).Count();

                totalAttendancePerClassList.Add(new totalAttendancePerClass
                {
                    ClassId = classes,
                    totalAttendance = totalAttendance
                });
            }

            var studentList = ent.Enrolments.Where(x => x.SubjectId == subjectInfo.SubjectId && x.SemesterId == semester.SemesterId && classIdList.Contains((int)x.Student.ClassId)).Include(x => x.Student).Include(x => x.Student.Class).OrderBy(x => x.Student.Class.Session).ThenBy(x => x.Student.Class.Group).ThenBy(x => x.Student.Name).ToList();

            var result = new List<LowAttendanceListInfo>();

            //Console.WriteLine(totalAttendance);
            //int attendance = 0;

            foreach (var student in studentList)
            {
                int attendance = ent.Attendances.Where(x => x.SubjectId == subjectInfo.SubjectId && x.Qrsession.QrsessionClasses.Any(qc => qc.ClassId == student.Student.ClassId) 
                    && x.Qrsession.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab") 
                    && x.MatricNo == student.MatricNo && x.IsValid == 1 && x.IsRegisteredStudent == 1 && x.Date >= semester.Week1Date && x.Date <= semester.Week15Date).Count();
                //Console.WriteLine(attendance);

                var totalSession = totalAttendancePerClassList.First(x => x.ClassId == student.Student.ClassId).totalAttendance;

                double attendanceRate = ((double)attendance / totalSession) * 100.0;
                if (attendanceRate > 100)
                {
                    attendanceRate = 100.0;
                }

                var className = subjectInfo.ClassInfo.First(x => x.ClassId == student.Student.ClassId).ClassName;

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

            return Ok(result);
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

        [HttpPost("RetrieveFullAttendanceList")]
        public IActionResult RetrieveFullAttendanceList([FromBody] SubjectInfo subjectInfo)
        {
            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);
            var week1 = semester.Week1Date;

            var classIdList = subjectInfo.ClassInfo.Select(x => x.ClassId).ToList();

            var totalAttendancePerClassList = new List<totalAttendancePerClass>();

            foreach (var classes in classIdList)
            {
                int totalAttendance = ent.Qrsessions.Where(s => s.SubjectId == subjectInfo.SubjectId && s.Status == 1 && s.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab") && s.CreatedTime >= semester.Week1Date && s.CreatedTime <= semester.Week15Date
                    && s.QrsessionClasses.Any(qc => qc.ClassId == classes)).Count();

                totalAttendancePerClassList.Add(new totalAttendancePerClass
                {
                    ClassId = classes,
                    totalAttendance = totalAttendance
                });
            }

            var studentList = ent.Enrolments.Where(x => x.SubjectId == subjectInfo.SubjectId && x.SemesterId == semester.SemesterId && classIdList.Contains((int)x.Student.ClassId)).Include(x => x.Student).Include(x => x.Student.Class).OrderBy(x => x.Student.Class.Session).ThenBy(x => x.Student.Class.Group).ThenBy(x => x.Student.Name).ToList();

            var result = new List<FullAttendanceListInfo>();

            foreach (var student in studentList)
            {
                int attendance = ent.Attendances.Where(x => x.SubjectId == subjectInfo.SubjectId && x.Qrsession.QrsessionClasses.Any(qc => qc.ClassId == student.Student.ClassId)
                    && x.Qrsession.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab")
                    && x.MatricNo == student.MatricNo && x.IsValid == 1 && x.IsRegisteredStudent == 1 && x.Date >= semester.Week1Date && x.Date <= semester.Week15Date).Count();

                var totalSession = totalAttendancePerClassList.First(x => x.ClassId == student.Student.ClassId).totalAttendance;

                double attendanceRate = ((double)attendance / totalSession) * 100;
                if (totalSession == 0)
                {
                    attendanceRate = 0;
                }
                if (attendanceRate > 100)
                {
                    attendanceRate = 100;
                }

                var studentClassId = student.Student.ClassId;
                var className = subjectInfo.ClassInfo.First(x => x.ClassId == studentClassId).ClassName;

                var weeklyAttendanceInfo = new List<WeeklyAttendanceInfo>();

                for (int week = 1; week <= totalWeek; week++)
                {
                    DateTime startDate = week1.AddDays((week - 1) * 7);
                    DateTime endDate = startDate.AddDays(6);

                    var totalAttendancePerWeek = (from s in ent.Qrsessions
                                           join sc in ent.QrsessionClasses
                                               on s.QrsessionId equals sc.QrsessionId
                                           where s.SubjectId == subjectInfo.SubjectId && s.Status == 1 && sc.ClassId == studentClassId && s.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab") && s.CreatedTime >= startDate && s.CreatedTime <= endDate
                                           select new
                                           {
                                               s.QrsessionId,
                                               s.SessionType
                                           }).ToList();

                    var attended = ent.Attendances.Where(a => a.SubjectId == subjectInfo.SubjectId && a.MatricNo == student.MatricNo && a.Qrsession.SessionType == (subjectInfo.isLecture ? "Lecture" : "Lab") && a.IsValid == 1 && a.IsRegisteredStudent == 1 && a.Date >= startDate && a.Date <= endDate)
                        .Select(x => new
                        {
                            x.QrsessionId,
                            x.Qrsession.SessionType,
                            x.Date,
                            x.Status,
                            x.IsValid
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

            return Ok(result);
        }

        public class AvailableSubjectList
        {
            public string SubjectName { get; set; }
            public string SubjectId { get; set; }
            public string LecturerName { get; set; }
            public int ActiveSemester { get; set; }
        }

        [HttpGet("RetrieveAvailableSubjectList")]
        public IActionResult RetrieveAvailableSubjectList()
        {
            var subjectList = ent.Subjects.Select(x => new
            {
                x.SubjectId,
                x.SubjectName,
                LecturerNameList = x.Teachings.Where(y => y.SubjectId == x.SubjectId).GroupBy(y => y.StaffId).Select(g => g.First().Staff.Name).ToList(),
                x.SemesterId
            }).ToList();

            var result = new List<AvailableSubjectList>();

            foreach (var subject in subjectList)
            {
                var lecturerName = string.Join('\n', subject.LecturerNameList);

                result.Add(new AvailableSubjectList
                {
                    SubjectId = subject.SubjectId,
                    SubjectName = subject.SubjectName,
                    LecturerName = lecturerName,
                    ActiveSemester = subject.SemesterId
                });
            }

            return Ok(result);
        }

        [HttpGet("RetrieveFacultyList")]
        public IActionResult RetrieveFacultyList()
        {
            var facultyList = ent.Courses.GroupBy(g => g.FacultyName).Select(x => x.First().FacultyName).ToList();

            return Ok(facultyList);
        }

        [HttpGet("RetrieveCourseList")]
        public IActionResult RetrieveCourseList(string facultyName)
        {
            var courseList = ent.Courses.Where(x => x.FacultyName == facultyName).Select(x => x.CourseId).ToList();

            return Ok(courseList);
        }

        [HttpGet("RetrieveStudentYearList")]
        public IActionResult RetrieveStudentYearList(string courseId)
        {
            var studentYearList = ent.Classes.Where(x => x.CourseId == courseId).GroupBy(x => x.Year).Select(g => g.First().Year).ToList();

            return Ok(studentYearList);
        }

        [HttpGet("RetrieveSemesterList")]
        public IActionResult RetrieveSemesterList()
        {
            var semesterList = ent.Semesters.Select(x => x.SemesterId).ToList();

            return Ok(semesterList);
        }

        [HttpGet("SubjectIdChecking")]
        public IActionResult SubjectIdChecking(string subjectID)
        {
            var subject = ent.Subjects.FirstOrDefault(x => x.SubjectId == subjectID);

            if (subject == null)
            {
                return Ok(new {Status = "valid"});
            }

            return Ok(new {Status = "invalid", Message = $"This Subject ID is occupied by {subject.SubjectName}." });
        }

        [HttpGet("RetrieveLecturerList")]
        public IActionResult RetrieveLecturerList()
        {
            var lecturerList = ent.Staff.Where(x => x.Role == "Lecturer").Select(x => new
            {
                LecturerId = x.StaffId,
                LecturerName = x.Name
            }).ToList();

            return Ok(lecturerList);
        }

        [HttpGet("RetrieveClassList")]
        public IActionResult RetrieveClassList(string courseId, int studentYear, string? subjectId)
        {
            var classList = ent.Classes.Where(x => x.CourseId == courseId && x.Year == studentYear).Select(x => new
            {
                x.ClassId,
                ClassName = "S" + x.Session + "G" + x.Group
            }).ToList();

            if (!string.IsNullOrEmpty(subjectId))
            {
                var subjectClassList = new List<TeachSubjectClassList>();
                foreach (var c in classList)
                {
                    var isSelected = ent.Teachings.FirstOrDefault(x => x.ClassId == c.ClassId && x.SubjectId == subjectId);

                    subjectClassList.Add(new TeachSubjectClassList
                    {
                        ClassId = c.ClassId,
                        ClassName = c.ClassName,
                        IsSelected = (isSelected != null) 
                    });
                }

                return Ok(subjectClassList);
            }

            return Ok(classList);
        }

        public class SubjectDetail
        {
            public string SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string CourseId { get; set; }
            public int StudentYear { get; set; }
            public int SemesterId { get; set; }

            public List<TeachSubjectList> teachingList { get; set; }

            public string? FacultyName { get; set; }
        }

        public class TeachSubjectList
        {
            //public string SubjectId { get; set; }

            public int LectureStatus { get; set; }
            public int LabStatus { get; set; } 

            public string StaffId { get; set; }
            public int ClassId { get; set; }

            // view subject detail member
            public string? LecturerName { get; set; }
            public string? ClassName { get; set; }
        }

        [HttpPost("AddSubject")]
        public IActionResult AddSubject([FromBody] SubjectDetail subjectDetail)
        {
            var subject = new Subject
            {
                SubjectId = subjectDetail.SubjectId,
                SubjectName = subjectDetail.SubjectName,
                CourseId = subjectDetail.CourseId,
                StudentYear = subjectDetail.StudentYear,
                SemesterId = subjectDetail.SemesterId
            };

            ent.Subjects.Add(subject);
            var subjectRow = ent.SaveChanges();

            if (subjectRow > 0)
            {
                var teachingList = new List<Teaching>();

                foreach (var classInfo in subjectDetail.teachingList)
                {
                    teachingList.Add(new Teaching
                    {
                        StaffId = classInfo.StaffId,
                        SubjectId = subjectDetail.SubjectId,
                        ClassId = classInfo.ClassId,
                        LectureStatus = classInfo.LectureStatus,
                        LabStatus = classInfo.LabStatus
                    });
                }

                ent.Teachings.AddRange(teachingList);
                var teachingRow = ent.SaveChanges();

                if (teachingRow > 0)
                {
                    return Ok(new { Status = "success", Message = "Success insert to subject and teaching table." });
                }
                else
                {
                    return Ok(new { Status = "failed", Message = "Failed to insert to subject table." });
                }
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to insert to subject table." });
            }
        }

        [HttpGet("RetrieveSubjectDetail")]
        public IActionResult RetrieveSubjectDetail(string subjectId)
        {
            var teachingInfo = ent.Teachings.Where(x => x.SubjectId == subjectId).Select(x => new
            {
                x.StaffId,
                LecturerName = x.Staff.Name,
                x.ClassId,
                ClassName = "S" + x.Class.Session + "G" + x.Class.Group,
                x.LectureStatus,
                x.LabStatus
            }).ToList();

            var teachingList = new List<TeachSubjectList>();

            foreach (var teacher in teachingInfo)
            {
                teachingList.Add(new TeachSubjectList
                {
                    StaffId = teacher.StaffId,
                    LecturerName = teacher.LecturerName,
                    ClassId = teacher.ClassId,
                    ClassName = teacher.ClassName,
                    LectureStatus = teacher.LectureStatus,
                    LabStatus = teacher.LabStatus
                });
            }

            var subjectInfo = ent.Subjects.Include(x => x.Course).First(x => x.SubjectId == subjectId);

            var subjectDetail = new SubjectDetail
            {
                FacultyName = subjectInfo.Course.FacultyName,
                CourseId = subjectInfo.CourseId,
                StudentYear = subjectInfo.StudentYear,
                SemesterId = subjectInfo.SemesterId,
                teachingList = teachingList
            };

            return Ok(subjectDetail);
        }

        public class DeleteSubjectList
        {
            public List<string> SubjectIdList { get; set; }
        }

        [HttpPost("DeleteSubject")]
        public IActionResult DeleteSubject([FromBody] DeleteSubjectList deleteSubjectList)
        {
            foreach (var subject in deleteSubjectList.SubjectIdList)
            {
                var subjectInfo = ent.Subjects.First(x => x.SubjectId == subject);

                ent.Subjects.Remove(subjectInfo);
            }    
            
            int subjectRow = ent.SaveChanges();

            if (subjectRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success delete subject." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to delete subject." });
            }
        }

        [HttpPost("EditSubject")]
        public IActionResult EditSubject(SubjectDetail subjectDetail)
        {
            var subject = ent.Subjects.First(x => x.SubjectId == subjectDetail.SubjectId);

            subject.SubjectName = subjectDetail.SubjectName;
            subject.CourseId = subjectDetail.CourseId;
            subject.StudentYear = subjectDetail.StudentYear;
            subject.SemesterId = subjectDetail.SemesterId;

            int subjectRow = ent.SaveChanges();

            if (subjectRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success edit subject." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to edit Subject Detail." });
            }
        }

        [HttpPost("EditSubjectTeachingList")]
        public IActionResult EditSubjectTeachingList(SubjectDetail subjectDetail)
        {
            var existingRecord = ent.Teachings.Where(x => x.SubjectId == subjectDetail.SubjectId).ToList();

            ent.Teachings.RemoveRange(existingRecord);

            var newRecord = subjectDetail.teachingList.Select(x => new Teaching
            {
                StaffId = x.StaffId,
                SubjectId = subjectDetail.SubjectId,
                ClassId = x.ClassId,
                LectureStatus = x.LectureStatus,
                LabStatus = x.LabStatus
            });

            ent.Teachings.AddRange(newRecord);

            int teachingRow = ent.SaveChanges();

            if (teachingRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success edit Subject teaching list." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to edit Subject teaching list." });
            }
        }

        public class EnrolmentInfo
        {
            public string SubjectId { get; set; }
            public List<string> StudentIdList { get; set; }
        }

        [HttpPost("AddEnrolment")]
        public IActionResult AddEnrolment([FromBody] EnrolmentInfo enrolmentInfo)
        {
            var semester = ent.Semesters.First(x => x.Week1Date <=  DateTime.Now && x.Week15Date >= DateTime.Now);

            var existingStudent = ent.Enrolments.Where(x => x.SubjectId == enrolmentInfo.SubjectId && x.SemesterId == semester.SemesterId).Select(x => x.MatricNo).ToList();

            var enrolmentList = new List<Enrolment>();

            var uniqueStudents = new List<string>();
            foreach (var s in enrolmentInfo.StudentIdList)
            {
                if (!uniqueStudents.Contains(s))
                    uniqueStudents.Add(s);
            }

            foreach (var student in uniqueStudents)
            {
                if (!existingStudent.Contains(student))
                {
                    enrolmentList.Add(new Enrolment
                    {
                        MatricNo = student,
                        SubjectId = enrolmentInfo.SubjectId,
                        Status = 1,
                        SemesterId = semester.SemesterId,
                        Year = semester.Year
                    });
                }               
            }

            ent.Enrolments.AddRange(enrolmentList);
            var enrolmentRow = ent.SaveChanges();

            if (enrolmentRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success insert to enrolment table." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to insert to enrolment table. \nMight because of the student already enrolled." });
            }
        }

        [HttpGet("RetriveTeachingSubjectInfo")]
        public IActionResult RetriveTeachingSubjectInfo(string staffId, string subjectId)
        {
            var subjectList = ent.Teachings.Where(x => x.StaffId == staffId && x.SubjectId == subjectId)
                .Select(x => new
                {
                    x.LectureStatus,
                    x.LabStatus
                }).ToList();

            bool lectureStatus = false;
            bool labStatus = false;

            if (subjectList.Any(x => x.LectureStatus == 1))
            {
                lectureStatus = true;
            }

            if (subjectList.Any(x => x.LabStatus == 1))
            {
                labStatus = true;
            }

            return Ok(new {LectureStatus = lectureStatus, LabStatus = labStatus});
        }

        [HttpGet("RetriveTeachingQRSessionInfo")]
        public IActionResult RetriveTeachingQRSessionInfo(string staffId, string subjectId, string sessionType)
        {
            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);
            var week1Date = semester.Week1Date;

            var subjectList = ent.Qrsessions.Where(x => x.StaffId == staffId && x.SubjectId == subjectId && x.SessionType == sessionType)
                .Select(x => new
                {
                    Date = x.CreatedTime.ToString("dd/MM/yyyy HH:mm"),
                    Week = ((x.CreatedTime.Date - week1Date).Days / 7) + 1,
                    x.QrsessionId
                }).ToList();

            return Ok(subjectList);
        }

        public class NotAttendStudentRequest
        {
            public string StaffId { get; set; }
            public string SubjectId { get; set; }
            public List<int> QrSessionId { get; set; }
        }

        [HttpPost("RetrieveNotAttendedStudentList")]
        public IActionResult RetrieveNotAttendedStudentList([FromBody] NotAttendStudentRequest notAttendStudentRequest)
        {
            var semester = ent.Semesters.First(x => x.Week1Date <= DateTime.Now && x.Week15Date >= DateTime.Now);

            var classIdList = ent.Teachings.Where(x => x.SubjectId == notAttendStudentRequest.SubjectId && x.StaffId == notAttendStudentRequest.StaffId).Select(x => x.ClassId).ToList();

            var studentList = ent.Enrolments.Where(x => x.SubjectId == notAttendStudentRequest.SubjectId && x.SemesterId == semester.SemesterId && classIdList.Contains((int)x.Student.ClassId)).Include(x => x.Student).OrderBy(x => x.MatricNo).ToList();

            var result = new List<LowAttendanceListInfo>();

            foreach (var student in studentList)
            {
                var attendance = ent.Attendances.FirstOrDefault(x => notAttendStudentRequest.QrSessionId.Contains(x.QrsessionId) && x.MatricNo == student.MatricNo);

                if (attendance == null)
                {
                    var studentClass = ent.Students.Include(x => x.Class).First(x => x.MatricNo == student.MatricNo).Class;
                    var className = "S" + studentClass.Session + "G" + studentClass.Group;
                    var studentInfo = new LowAttendanceListInfo
                    {
                        StudentId = student.MatricNo,
                        StudentName = student.Student.Name,
                        ClassName = className
                    };
                    result.Add(studentInfo);
                }
            }

            return Ok(result);
        }

        [HttpGet("EditStudentAttendanceStatus")]
        public IActionResult EditStudentAttendanceStatus(string studentId, int qrSessionId, string reason)
        {
            var sessionInfo = ent.Qrsessions.First(x => x.QrsessionId == qrSessionId);

            var attendance = new Attendance
            {
                SubjectId = sessionInfo.SubjectId,
                QrsessionId = qrSessionId,
                MatricNo = studentId,
                Date = sessionInfo.CreatedTime,
                Status = "Absent with Reason",
                Location = reason,
                IsValid = 1,
                IsRegisteredStudent = 1
            };

            ent.Attendances.Add(attendance);
            var enrolmentRow = ent.SaveChanges();

            if (enrolmentRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success insert to attendance table." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to insert to attendance table. " });
            }
        }

        [HttpGet("VerifyCode")]
        public IActionResult VerifyCode(string emailAddress, string verificationCode)
        {
            var passwordReset = ent.PasswordResets.FirstOrDefault(x => x.Email == emailAddress 
                && x.VerificationCode == verificationCode);

            if (passwordReset == null)
            {
                return Ok(new { Status = "failed", Message = "Verification code does not match." });
            }

            if (passwordReset.ExpireTime < DateTime.Now)
            {
                return Ok(new { Status = "failed", Message = "Verification code has expired." });
            }

            var userRole = passwordReset.MatricNo != null ? "Student" : "Staff";
            var userId = passwordReset.MatricNo != null ? passwordReset.MatricNo : passwordReset.StaffId;

            return Ok(new { UserRole = userRole, UserID = userId, Status = "success", Message = "Verification code matches." });
        }

        public class UpdatePasswordInfo
        {
            public string UserRole { get; set; }
            public string UserID { get; set; }
            public string Password { get; set; }
        }

        [HttpPost("UpdatePassword")]
        public IActionResult UpdatePassword([FromBody] UpdatePasswordInfo updatePasswordInfo)
        {
            int dbChangesRow = 0;
            bool samePassword = false;

            if (updatePasswordInfo.UserRole == "Student")
            {
                var student = ent.Students.First(x => x.MatricNo == updatePasswordInfo.UserID);

                if (student.Password == updatePasswordInfo.Password)
                {
                    return Ok(new { Status = "failed", Message = "New password cannot same with old password. " });
                }

                student.Password = updatePasswordInfo.Password;

                dbChangesRow = ent.SaveChanges();
            }
            else
            {
                var staff = ent.Staff.First(x => x.StaffId == updatePasswordInfo.UserID);

                if (staff.Password == updatePasswordInfo.Password)
                {
                    return Ok(new { Status = "failed", Message = "New password cannot same with old password. " });
                }

                staff.Password = updatePasswordInfo.Password;

                dbChangesRow = ent.SaveChanges();
            }

            if (dbChangesRow > 0)
            {
                return Ok(new { Status = "success", Message = "Success update user table." });
            }
            else
            {
                return Ok(new { Status = "failed", Message = "Failed to update user table. " });
            }
        }
    }
}
