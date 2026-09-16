using FYP_App.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using ZXing.PDF417.Internal;


namespace FYP_App.Service
{
    public class ApiService
    {
        //public string URL { get; set; } = "http://10.0.2.2:5150/api/"; 
        public string URL { get; set; } = "http://10.131.72.25:5150/api/"; 
        public HttpClient client {  get; set; }
        public ApiService() 
        {
            client = new HttpClient();
        }

        public async Task<bool> LoginStudent(string userid, string password)
        {
            var loginData = new
            {
                userid = userid,
                password = password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "LoginStudent", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(result);
                App.User = user;
                return true;
            }

            return false;
        }

        public async Task<bool> LoginStaff(string userid, string password)
        {
            var loginData = new
            {
                userid = userid,
                password = password
            };

            var json = JsonConvert.SerializeObject(loginData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = URL + "values/";
            var response = await client.PostAsync(url + "LoginStaff", content);

            if (response.IsSuccessStatusCode)
            {
                string result = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<User>(result);
                App.User = user;
                return true;
            }

            return false;
        }

        public async Task<List<TeachSubjectList>> TeachingSubjectList(string userId)
        {
            var url = this.URL + $"values/TeachingSubjectList?staffId={userId}";
            
            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TeachSubjectList>>(result);
        }

        public async Task<CurrentActiveSession> CurrentActiveSession(string userId, string subjectId)
        {
            var url = this.URL + $"values/CurrentActiveSession?staffId={userId}&subjectId={subjectId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CurrentActiveSession>(result);
        }

        public async Task<bool> UpdateStudentAttendanceValidility(UpdateStudentAttendanceValidation updateStudentAttendanceValidation)
        {
            var json = JsonConvert.SerializeObject(updateStudentAttendanceValidation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "UpdateStudentAttendanceValidility", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        public async Task<List<TeachSubjectClassList>> TeachSubjectLectureClassList(string staffID, string role, string subjectID)
        {
            var url = this.URL + $"values/TeachSubjectLectureClassList?staffId={staffID}&role={role}&subjectId={subjectID}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TeachSubjectClassList>>(result);
        }

        public async Task<List<TeachSubjectClassList>> TeachSubjectLabClassList(string staffID, string role, string subjectID)
        {
            var url = this.URL + $"values/TeachSubjectLabClassList?staffId={staffID}&role={role}&subjectId={subjectID}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TeachSubjectClassList>>(result);
        }

        public async Task<int?> GenerateQrCode(GenerateQRCodeInfo generateQRCodeInfo)
        {
            var json = JsonConvert.SerializeObject(generateQRCodeInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "GenerateQrCode", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<int>(result);
            }

            return null;
        }

        public async Task<bool> ReplaceQrCode(GenerateQRCodeInfo generateQRCodeInfo, int qrSessionId)
        {
            var json = JsonConvert.SerializeObject(generateQRCodeInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = this.URL + $"values/ReplaceQrCode?qrSessionId={qrSessionId}";

            var response = await client.PostAsync(url + "ReplaceQrCode", content);

            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }

        public async Task<GenerateQRCodeInfo> RetrieveExistingQrCode(int qrSessionId)
        {
            var url = this.URL + $"values/RetrieveExistingQrCode?qrSessionId={qrSessionId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<GenerateQRCodeInfo>(result);
        }

        public async Task<StatusClass> ValidateStudentDevice(string studentId, string deviceId, string deviceName)
        {
            var url = this.URL + $"values/ValidateStudentDevice?studentId={studentId}&deviceId={deviceId}&deviceName={deviceName}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            var result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> AddAttendance(AddAttendanceRecord addAttendanceRecord)
        {
            var json = JsonConvert.SerializeObject(addAttendanceRecord);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "AddAttendance", content);

            //if (!response.IsSuccessStatusCode)
            //{
            //    Console.WriteLine("Not success");
            //}
            //else
            //{
            //    Console.WriteLine("Success");
            //}

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> RetrieveStudentName(string studentId)
        {
            var url = this.URL + $"values/RetrieveStudentName?studentId={studentId}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<List<TodayAttendance>> RetrieveStudentTodayAttendance(string studentId)
        {
            var url = this.URL + $"values/RetrieveStudentTodayAttendance?studentId={studentId}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TodayAttendance>>(result);
        }

        public async Task<List<StudentAttendanceGraph>> RetrieveStudentAttendanceGraph(string studentId, int classId)
        {
            var url = this.URL + $"values/RetrieveStudentAttendanceGraph?studentId={studentId}&classId={classId}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<StudentAttendanceGraph>>(result);
        }

        public async Task<List<StudentAttendanceDetail>> RetrieveStudentAttendanceDetail(string studentId, int classId, string subjectId, int semesterId)
        {
            var url = this.URL + $"values/RetrieveStudentAttendanceDetail?studentId={studentId}&classId={classId}&subjectId={subjectId}&semesterId={semesterId}";

            var res = await client.GetAsync(url);
            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<StudentAttendanceDetail>>(result);
        }

        public async Task<List<Report_SubjectList>> RetrieveReportSubjectList(string staffId, string role)
        {
            var url = this.URL + $"values/RetrieveReportSubjectList?staffId={staffId}&role={role}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Report_SubjectList>>(result);
        }

        public async Task<List<AttendanceListInfo>> RetrieveLowAttendanceList(SubjectInfo subjectInfo)
        {
            var json = JsonConvert.SerializeObject(subjectInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "RetrieveLowAttendanceList", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AttendanceListInfo>>(result);
            }

            return null;
        }

        public async Task<string> DownloadCsv_LowAttendance_Current(DownloadReportInfo results)
        {
            var json = JsonConvert.SerializeObject(results);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "home/";
            var response = await client.PostAsync(url + "DownloadCsv_LowAttendance_Current", content);

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Low Attendance Report - Current_{results.SubjectId}_{results.SessionType}.csv";

                // Save file on device but is inside app directory is not accessible by user
                //var filePath = Path.Combine(FileSystem.AppDataDirectory, "result.csv");
                //File.WriteAllBytes(filePath, fileBytes);

                // Check Android version
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10)
                {
                    // Android 9 and below: request StorageWrite permission
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied",
                            "Cannot save file without storage permission.", "OK");
                        return null;
                    }
                }

                var fileSaver = DependencyService.Get<ISaveFileInPhone>();

                bool success = await fileSaver.SaveFileToDownloads(fileBytes, fileName);

                if (success)
                {
                    return fileName;
                }
                else
                {
                    return null;
                }
                // Optional: open the file
                //await Launcher.OpenAsync(filePath);
            }

            return null;
        }

        public async Task<string> DownloadCsv_LowAttendance_Schedule(DownloadScheduleReportInfo results)
        {
            var json = JsonConvert.SerializeObject(results);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "home/";
            var response = await client.PostAsync(url + "DownloadCsv_LowAttendance_Schedule", content);

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Low Attendance Report - Week {results.WeekNum}_{results.SubjectId}_{results.SessionType}.csv";

                // Save file on device but is inside app directory is not accessible by user
                //var filePath = Path.Combine(FileSystem.AppDataDirectory, "result.csv");
                //File.WriteAllBytes(filePath, fileBytes);

                // Check Android version
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10)
                {
                    // Android 9 and below: request StorageWrite permission
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied",
                            "Cannot save file without storage permission.", "OK");
                        return null;
                    }
                }

                var fileSaver = DependencyService.Get<ISaveFileInPhone>();

                bool success = await fileSaver.SaveFileToDownloads(fileBytes, fileName);

                if (success)
                {
                    return fileName;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public async Task<List<AttendanceListInfo>> RetrieveFullAttendanceList(SubjectInfo subjectInfo)
        {
            var json = JsonConvert.SerializeObject(subjectInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "RetrieveFullAttendanceList", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<AttendanceListInfo>>(result);
            }

            return null;
        }

        public async Task<string> DownloadCsv_FullAttendance_Current(DownloadFullReportInfo results)
        {
            var json = JsonConvert.SerializeObject(results);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "home/";
            var response = await client.PostAsync(url + "DownloadCsv_FullAttendance_Current", content);

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Full Attendance Report - Current_{results.SubjectId}_{results.SessionType}.csv";

                // Save file on device but is inside app directory is not accessible by user
                //var filePath = Path.Combine(FileSystem.AppDataDirectory, "result.csv");
                //File.WriteAllBytes(filePath, fileBytes);

                // Check Android version
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10)
                {
                    // Android 9 and below: request StorageWrite permission
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied",
                            "Cannot save file without storage permission.", "OK");
                        return null;
                    }
                }

                var fileSaver = DependencyService.Get<ISaveFileInPhone>();

                bool success = await fileSaver.SaveFileToDownloads(fileBytes, fileName);

                if (success)
                {
                    return fileName;
                }
                else
                {
                    return null;
                }
                // Optional: open the file
                //await Launcher.OpenAsync(filePath);
            }

            return null;
        }

        public async Task<string> DownloadCsv_FullAttendance_Schedule(DownloadScheduleReportInfo results)
        {
            var json = JsonConvert.SerializeObject(results);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "home/";
            var response = await client.PostAsync(url + "DownloadCsv_FullAttendance_Schedule", content);

            if (response.IsSuccessStatusCode)
            {
                var fileBytes = await response.Content.ReadAsByteArrayAsync();
                var fileName = $"Full Attendance Report - Week {results.WeekNum}_{results.SubjectId}_{results.SessionType}.csv";

                // Save file on device but is inside app directory is not accessible by user
                //var filePath = Path.Combine(FileSystem.AppDataDirectory, "result.csv");
                //File.WriteAllBytes(filePath, fileBytes);

                // Check Android version
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10)
                {
                    // Android 9 and below: request StorageWrite permission
                    var status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status != PermissionStatus.Granted)
                        status = await Permissions.RequestAsync<Permissions.StorageWrite>();

                    if (status != PermissionStatus.Granted)
                    {
                        await Application.Current.MainPage.DisplayAlert("Permission Denied",
                            "Cannot save file without storage permission.", "OK");
                        return null;
                    }
                }

                var fileSaver = DependencyService.Get<ISaveFileInPhone>();

                bool success = await fileSaver.SaveFileToDownloads(fileBytes, fileName);

                if (success)
                {
                    return fileName;
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        public async Task<List<AvailableSubjectList>> RetrieveAvailableSubjectList()
        {
            var url = this.URL + $"values/RetrieveAvailableSubjectList";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<AvailableSubjectList>>(result);
        }

        public async Task<List<string>> RetrieveFacultyList()
        {
            var url = this.URL + $"values/RetrieveFacultyList";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(result);
        }

        public async Task<List<string>> RetrieveCourseList(string facultyName)
        {
            var url = this.URL + $"values/RetrieveCourseList?facultyName={facultyName}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<string>>(result);
        }

        public async Task<List<int>> RetrieveStudentYearList(string courseId)
        {
            var url = this.URL + $"values/RetrieveStudentYearList?courseId={courseId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<int>>(result);
        }

        public async Task<List<int>> RetrieveSemesterList()
        {
            var url = this.URL + $"values/RetrieveSemesterList";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<int>>(result);
        }

        public async Task<StatusClass> SubjectIdChecking(string subjectID)
        {
            var url = this.URL + $"values/SubjectIdChecking?subjectID={subjectID}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<List<LecturerList>> RetrieveLecturerList()
        {
            var url = this.URL + $"values/RetrieveLecturerList";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<LecturerList>>(result);
        }

        public async Task<List<TeachSubjectClassList>> RetrieveClassList(string courseId, int studentYear, string subjectId)
        {
            var url = this.URL + $"values/RetrieveClassList?courseId={courseId}&studentYear={studentYear}&subjectId={subjectId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TeachSubjectClassList>>(result);
        }

        public async Task<StatusClass> AddSubject(SubjectDetail subjectDetail)
        {
            var json = JsonConvert.SerializeObject(subjectDetail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "AddSubject", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<SubjectDetail> RetrieveSubjectDetail(string subjectId)
        {
            var url = this.URL + $"values/RetrieveSubjectDetail?subjectId={subjectId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<SubjectDetail>(result);
        }

        public async Task<StatusClass> DeleteSubject(DeleteSubjectList deleteSubjectList)
        {
            var json = JsonConvert.SerializeObject(deleteSubjectList);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "DeleteSubject", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> EditSubject(SubjectDetail subjectDetail)
        {
            var json = JsonConvert.SerializeObject(subjectDetail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "EditSubject", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> EditSubjectTeachingList(SubjectDetail subjectDetail)
        {
            var json = JsonConvert.SerializeObject(subjectDetail);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "EditSubjectTeachingList", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<List<StudentCheckResult>> UploadCSVFile(string filePath)
        {
            var content = new MultipartFormDataContent();

            var stream = File.OpenRead(filePath);
            content.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));

            var url = URL + "home/";
            var response = await client.PostAsync(url + "UploadCSVFile", content);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("CSV Error", result, "OK");
                return null;
            }

            return JsonConvert.DeserializeObject<List<StudentCheckResult>>(result);
        }

        public async Task<StatusClass> AddEnrolment(EnrolmentInfo enrolmentInfo)
        {
            var json = JsonConvert.SerializeObject(enrolmentInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "AddEnrolment", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> RetriveTeachingSubjectInfo(string staffId, string subjectId)
        {
            var url = this.URL + $"values/RetriveTeachingSubjectInfo?staffId={staffId}&subjectId={subjectId}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<List<TeachingQRSessionInfo>> RetriveTeachingQRSessionInfo(string staffId, string subjectId, string sessionType)
        {
            var url = this.URL + $"values/RetriveTeachingQRSessionInfo?staffId={staffId}&subjectId={subjectId}&sessionType={sessionType}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TeachingQRSessionInfo>>(result);
        }

        public async Task<List<AttendanceListInfo>> RetrieveNotAttendedStudentList(NotAttendStudentRequest notAttendStudentRequest)
        {
            var json = JsonConvert.SerializeObject(notAttendStudentRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "RetrieveNotAttendedStudentList", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<List<AttendanceListInfo>>(result);
        }

        public async Task<StatusClass> UploadPDFFile(string filePath)
        {
            var content = new MultipartFormDataContent();

            var stream = File.OpenRead(filePath);
            content.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));

            var url = URL + "home/";
            var response = await client.PostAsync(url + "UploadPDFFile", content);

            var result = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> EditStudentAttendanceStatus(string studentId, int qrSessionId, string reason)
        {
            var url = this.URL + $"values/EditStudentAttendanceStatus?studentId={studentId}&qrSessionId={qrSessionId}&reason={reason}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> SendEmail(EmailRequest emailRequest)
        {
            var json = JsonConvert.SerializeObject(emailRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "email/";
            var response = await client.PostAsync(url + "SendEmail", content);

            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await Application.Current.MainPage.DisplayAlert("Email Error", result, "OK");
                return null;
            }

            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> VerifyCode(string emailAddress, string verificationCode)
        {
            var url = this.URL + $"values/VerifyCode?emailAddress={emailAddress}&verificationCode={verificationCode}";

            var res = await client.GetAsync(url);

            if (!res.IsSuccessStatusCode)
            {
                return null;
            }

            string result = await res.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }

        public async Task<StatusClass> UpdatePassword(UpdatePasswordInfo updatePasswordInfo)
        {
            var json = JsonConvert.SerializeObject(updatePasswordInfo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = URL + "values/";
            var response = await client.PostAsync(url + "UpdatePassword", content);

            var result = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<StatusClass>(result);
        }
    }
}
