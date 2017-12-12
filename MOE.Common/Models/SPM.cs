namespace MOE.Common.Models
{
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System.Data.Entity.Infrastructure.Annotations;

    public partial class SPM : IdentityDbContext<MOE.Common.Business.SiteSecurity.SPMUser>
    {
         public SPM()
            : base("name=SPM")
        {
            Database.SetInitializer<SPM>(new CreateDatabaseIfNotExists<SPM>());
        }

        public static MOE.Common.Models.SPM Create()
        {
            return new MOE.Common.Models.SPM();
        }


       
        public System.Data.Entity.DbSet<MOE.Common.Business.SiteSecurity.SPMRole> IdentityRoles { get; set; }
        
        public virtual DbSet<ApplicationEvent> ApplicationEvents { get; set; }
        public virtual DbSet<SPMWatchDogErrorEvent> SPMWatchDogErrorEvents { get; set; }
        public virtual DbSet<MetricComment> MetricComments { get; set; }
        public virtual DbSet<DetectorComment> DetectorComments { get; set; }
        public virtual DbSet<MovementType> MovementTypes { get; set; }
        public virtual DbSet<DirectionType> DirectionTypes { get; set; }
        public virtual DbSet<LaneType> LaneTypes { get; set; }
        public virtual DbSet<Approach> Approaches { get; set; }
        public virtual DbSet<DetectionType> DetectionTypes { get; set; }
        public virtual DbSet<MetricsFilterType> MetricsFilterTypes { get; set; }
        public virtual DbSet<MetricType> MetricTypes { get; set; }
        public virtual DbSet<Controller_Event_Log> Controller_Event_Log { get; set; }        
        public virtual DbSet<Agency> Agencies { get; set; }
        public virtual DbSet<Detector> Detectors { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<Region> Regions { get; set; }
        public virtual DbSet<Signal> Signals { get; set; }
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<ActionLog> ActionLogs { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<RouteSignal> RouteSignals { get; set; }
        public virtual DbSet<RoutePhaseDirection> RoutePhaseDirections { get; set; }
        public virtual DbSet<ControllerType> ControllerType { get; set; }
        public virtual DbSet<Speed_Events> Speed_Events { get; set; }
        public virtual DbSet<FAQ> FAQs { get; set; }
        public virtual DbSet<ExternalLink> ExternalLinks { get; set; }
        public virtual DbSet<Application> Applications { get; set; }
        public virtual DbSet<ApplicationSettings> ApplicationSettings { get; set; }
        public virtual DbSet<WatchDogApplicationSettings> WatchdogApplicationSettings { get; set; }
        public virtual DbSet<DetectionHardware> DetectionHardwares { get; set; }
        public virtual DbSet<VersionAction> VersionActions { get; set; }
        public virtual DbSet<PreemptionAggregation> PreemptionAggregations { get; set; }
        public virtual DbSet<PriorityAggregation> PriorityAggregations { get; set; }
        public virtual DbSet<ApproachCycleAggregation> ApproachCycleAggregations { get; set; }
        public virtual DbSet<ApproachPcdAggregation> ApproachPcdAggregations { get; set; }
        public virtual DbSet<ApproachSplitFailAggregation> ApproachSplitFailAggregations { get; set; }
        public virtual DbSet<ApproachYellowRedActivationAggregation> ApproachYellowRedActivationAggregations { get; set; }
        public virtual DbSet<ApproachSpeedAggregation> ApproachSpeedAggregations { get; set; }
        public virtual DbSet<DetectorAggregation> DetectorAggregations { get; set; }



        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Conventions.Remove<ManyToManyCascadeDeleteConvention>();
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();
            base.OnModelCreating(modelBuilder);  

            modelBuilder.Entity<Signal>()
                .Property(e => e.PrimaryName)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .Property(e => e.SecondaryName)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .Property(e => e.IPAddress)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .Property(e => e.Latitude)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .Property(e => e.Longitude)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .Property(e => e.RegionID);

            modelBuilder.Entity<Approach>()
                .HasMany(e => e.Detectors)
                .WithRequired(e => e.Approach)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Detector>()
                .HasMany(e => e.DetectorComments)
                .WithRequired(e => e.Detector)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Detector>()
                .Property(e => e.DetectorID)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("IX_DetectorIDUnique") { IsUnique = true }));
            

            modelBuilder.Entity<ControllerType>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<ControllerType>()
                .Property(e => e.FTPDirectory)
                .IsUnicode(false);

            modelBuilder.Entity<ControllerType>()
                .Property(e => e.UserName)
                .IsUnicode(false);

            modelBuilder.Entity<ControllerType>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Signal>()
                .HasRequired(s => s.ControllerType)
                .WithMany()
                .HasForeignKey(u => u.ControllerTypeID).WillCascadeOnDelete(false);

            modelBuilder.Entity<Detector>()
                .HasMany(g => g.DetectionTypes)
                .WithMany(d => d.Detectors)
                .Map(mc =>
                {
                    mc.ToTable("DetectionTypeDetector");
                    mc.MapLeftKey("ID");
                    mc.MapRightKey("DetectionTypeID");
                }
                );

            modelBuilder.Entity<ActionLog>()
                .HasMany(al => al.Actions);
            modelBuilder.Entity<ActionLog>()
                .HasMany(al => al.MetricTypes);
        }

        //public System.Data.Entity.DbSet<SPM.Models.AggDataExportViewModel> AggDataExportViewModels { get; set; }
    }
}
