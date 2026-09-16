using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WebApi2.Models;

public partial class UTeM_Student_Attendance_AppEntities : DbContext
{
    public UTeM_Student_Attendance_AppEntities()
    {
    }

    public UTeM_Student_Attendance_AppEntities(DbContextOptions<UTeM_Student_Attendance_AppEntities> options)
        : base(options)
    {
    }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<Enrolment> Enrolments { get; set; }

    public virtual DbSet<PasswordReset> PasswordResets { get; set; }

    public virtual DbSet<Qrsession> Qrsessions { get; set; }

    public virtual DbSet<QrsessionClass> QrsessionClasses { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teaching> Teachings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=JIANG;Initial Catalog=UTeM Student Attendance App;Integrated Security=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => new { e.SubjectId, e.QrsessionId, e.MatricNo }).HasName("PK__Attendan__4104FD1146EFB6BA");

            entity.ToTable("Attendance");

            entity.Property(e => e.SubjectId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SubjectID");
            entity.Property(e => e.QrsessionId).HasColumnName("QRSessionID");
            entity.Property(e => e.MatricNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.IsValid).HasDefaultValue(1);
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Status)
                .HasMaxLength(18)
                .IsUnicode(false);

            entity.HasOne(d => d.MatricNoNavigation).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.MatricNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__Matri__5629CD9C");

            entity.HasOne(d => d.Qrsession).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.QrsessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Attendanc__QRSes__5535A963");

            entity.HasOne(d => d.Subject).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_Attendance_Subject");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Class__CB1927A0CEDBE73F");

            entity.ToTable("Class");

            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CourseID");

            entity.HasOne(d => d.Course).WithMany(p => p.Classes)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Class__CourseID__398D8EEE");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__C92D7187ECB63DA3");

            entity.ToTable("Course");

            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.FacultyName)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("PK__Device__49E12331B051AE44");

            entity.ToTable("Device");

            entity.Property(e => e.DeviceId).HasColumnName("DeviceID");
            entity.Property(e => e.AndroidId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("AndroidID");
            entity.Property(e => e.DeviceName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastUsed).HasColumnType("datetime");
            entity.Property(e => e.MatricNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();

            entity.HasOne(d => d.MatricNoNavigation).WithMany(p => p.Devices)
                .HasForeignKey(d => d.MatricNo)
                .HasConstraintName("FK__Device__MatricNo__4CA06362");
        });

        modelBuilder.Entity<Enrolment>(entity =>
        {
            entity.HasKey(e => e.EnrolmentId).HasName("PK__Enrolmen__5C0E5FEF1907C793");

            entity.ToTable("Enrolment");

            entity.Property(e => e.EnrolmentId).HasColumnName("EnrolmentID");
            entity.Property(e => e.MatricNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.SemesterId).HasColumnName("SemesterID");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SubjectID");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.MatricNo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrolment__Matri__48CFD27E");

            entity.HasOne(d => d.Semester).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrolment_Semester");

            entity.HasOne(d => d.Subject).WithMany(p => p.Enrolments)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_Enrolment_Subject");
        });

        modelBuilder.Entity<PasswordReset>(entity =>
        {
            entity.HasKey(e => e.ResetId).HasName("PK__Password__783CF7AD2C804469");

            entity.ToTable("PasswordReset");

            entity.Property(e => e.ResetId).HasColumnName("ResetID");
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ExpireTime).HasColumnType("datetime");
            entity.Property(e => e.MatricNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("StaffID");
            entity.Property(e => e.VerificationCode)
                .HasMaxLength(6)
                .IsUnicode(false);

            entity.HasOne(d => d.MatricNoNavigation).WithMany(p => p.PasswordResets)
                .HasForeignKey(d => d.MatricNo)
                .HasConstraintName("FK__PasswordR__Matri__6383C8BA");

            entity.HasOne(d => d.Staff).WithMany(p => p.PasswordResets)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__PasswordR__Staff__6477ECF3");
        });

        modelBuilder.Entity<Qrsession>(entity =>
        {
            entity.HasKey(e => e.QrsessionId).HasName("PK__QRSessio__5910B2677DCDC72F");

            entity.ToTable("QRSession");

            entity.Property(e => e.QrsessionId).HasColumnName("QRSessionID");
            entity.Property(e => e.CreatedTime).HasColumnType("datetime");
            entity.Property(e => e.ExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.Qrcode)
                .HasMaxLength(36)
                .IsUnicode(false)
                .HasColumnName("QRCode");
            entity.Property(e => e.SessionType)
                .HasMaxLength(7)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("StaffID");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SubjectID");

            entity.HasOne(d => d.Staff).WithMany(p => p.Qrsessions)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QRSession__Staff__5070F446");

            entity.HasOne(d => d.Subject).WithMany(p => p.Qrsessions)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_QRSession_Subject");
        });

        modelBuilder.Entity<QrsessionClass>(entity =>
        {
            entity.HasKey(e => e.QrclassId).HasName("PK__QRSessio__82CA822834421839");

            entity.ToTable("QRSessionClass");

            entity.Property(e => e.QrclassId).HasColumnName("QRClassID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.QrsessionId).HasColumnName("QRSessionID");

            entity.HasOne(d => d.Class).WithMany(p => p.QrsessionClasses)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QRSession__Class__68487DD7");

            entity.HasOne(d => d.Qrsession).WithMany(p => p.QrsessionClasses)
                .HasForeignKey(d => d.QrsessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__QRSession__QRSes__6754599E");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId).HasName("PK__Semester__043301BD92CA6421");

            entity.ToTable("Semester");

            entity.Property(e => e.SemesterId)
                .ValueGeneratedNever()
                .HasColumnName("SemesterID");
            entity.Property(e => e.Week15Date).HasColumnType("datetime");
            entity.Property(e => e.Week1Date).HasColumnType("datetime");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Staff__96D4AAF704EA6DD6");

            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("StaffID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Role)
                .HasMaxLength(8)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.MatricNo).HasName("PK__Student__8E55BF896D0FA248");

            entity.ToTable("Student");

            entity.Property(e => e.MatricNo)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.ClassId).HasColumnName("ClassID");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Student__ClassID__3C69FB99");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubjectId).HasName("PK__Subject__AC1BA388BB1E6E61");

            entity.ToTable("Subject");

            entity.Property(e => e.SubjectId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SubjectID");
            entity.Property(e => e.CourseId)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("CourseID");
            entity.Property(e => e.SemesterId).HasColumnName("SemesterID");
            entity.Property(e => e.SubjectName)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.CourseId)
                .HasConstraintName("FK__Subject__CourseI__412EB0B6");

            entity.HasOne(d => d.Semester).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.SemesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subject_Semester");
        });

        modelBuilder.Entity<Teaching>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.SubjectId, e.ClassId }).HasName("PK__Teaching__ADDE09E89A6839AD");

            entity.ToTable("Teaching");

            entity.Property(e => e.StaffId)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("StaffID");
            entity.Property(e => e.SubjectId)
                .HasMaxLength(8)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SubjectID");
            entity.Property(e => e.ClassId).HasColumnName("ClassID");

            entity.HasOne(d => d.Class).WithMany(p => p.Teachings)
                .HasForeignKey(d => d.ClassId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teaching__ClassI__45F365D3");

            entity.HasOne(d => d.Staff).WithMany(p => p.Teachings)
                .HasForeignKey(d => d.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Teaching__StaffI__440B1D61");

            entity.HasOne(d => d.Subject).WithMany(p => p.Teachings)
                .HasForeignKey(d => d.SubjectId)
                .HasConstraintName("FK_Teaching_Subject");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
