using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfrastructureLayer.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConcessionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Price = table.Column<double>(type: "double precision", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConcessionItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_actors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_actors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_cinema_rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    NumberOfRows = table.Column<int>(type: "integer", nullable: false),
                    NumberOfColumns = table.Column<int>(type: "integer", nullable: false),
                    DefaultSeatPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_cinema_rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_directors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_directors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdentityCard = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Gender = table.Column<int>(type: "integer", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "date", nullable: false),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Salary = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "date", nullable: false),
                    ProfileImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_movies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductionCompany = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RunningTime = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: true),
                    Actors = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Director = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TrailerUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false),
                    IsRecommended = table.Column<bool>(type: "boolean", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DiscountPercent = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    IdentityCard = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Score = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    BirthDate = table.Column<DateTime>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Avatar = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_seats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SeatType = table.Column<int>(type: "integer", nullable: false),
                    RowIndex = table.Column<int>(type: "integer", nullable: false),
                    ColumnIndex = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    PriceSeat = table.Column<decimal>(type: "numeric", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_seats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_seats_tbl_cinema_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "tbl_cinema_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_movie_actors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_movie_actors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_movie_actors_tbl_actors_ActorId",
                        column: x => x.ActorId,
                        principalTable: "tbl_actors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_movie_actors_tbl_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "tbl_movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_movie_directors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    DirectorId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_movie_directors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_movie_directors_tbl_directors_DirectorId",
                        column: x => x.DirectorId,
                        principalTable: "tbl_directors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_movie_directors_tbl_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "tbl_movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_movie_genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    GenreId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_movie_genres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_movie_genres_tbl_genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "tbl_genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_movie_genres_tbl_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "tbl_movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_movie_images",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_movie_images", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_movie_images_tbl_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "tbl_movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_show_times",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    MovieId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_show_times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_show_times_tbl_cinema_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "tbl_cinema_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_show_times_tbl_movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "tbl_movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_user_promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRedeemed = table.Column<bool>(type: "boolean", nullable: false),
                    RedeemedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_user_promotions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_user_promotions_tbl_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "tbl_promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_user_promotions_tbl_users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IdentityCardNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookingCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalSeats = table.Column<int>(type: "integer", nullable: false),
                    ConvertedTickets = table.Column<int>(type: "integer", nullable: true),
                    PointsUsed = table.Column<double>(type: "double precision", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_bookings_tbl_show_times_ShowTimeId",
                        column: x => x.ShowTimeId,
                        principalTable: "tbl_show_times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_bookings_tbl_users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_seat_log",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_seat_log", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_seat_log_tbl_seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "tbl_seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_seat_log_tbl_show_times_ShowTimeId",
                        column: x => x.ShowTimeId,
                        principalTable: "tbl_show_times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_seat_log_tbl_users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_booking_details",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeatId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_booking_details", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_booking_details_tbl_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "tbl_bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_booking_details_tbl_seats_SeatId",
                        column: x => x.SeatId,
                        principalTable: "tbl_seats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_point_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Points = table.Column<double>(type: "double precision", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_point_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_point_histories_tbl_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "tbl_bookings",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_tbl_point_histories_tbl_users_UserId",
                        column: x => x.UserId,
                        principalTable: "tbl_users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_tickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovieName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Screen = table.Column<string>(type: "text", nullable: false),
                    ShowDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ShowTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    SeatCode = table.Column<string>(type: "text", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShowTimeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_tickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_tickets_tbl_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "tbl_bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_tickets_tbl_show_times_ShowTimeId",
                        column: x => x.ShowTimeId,
                        principalTable: "tbl_show_times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_transaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    MerchantTransactionId = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_transaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_transaction_tbl_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "tbl_bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tbl_booking_details_BookingId",
                table: "tbl_booking_details",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_booking_details_SeatId",
                table: "tbl_booking_details",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_bookings_ShowTimeId",
                table: "tbl_bookings",
                column: "ShowTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_bookings_UserId",
                table: "tbl_bookings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_actors_ActorId",
                table: "tbl_movie_actors",
                column: "ActorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_actors_MovieId",
                table: "tbl_movie_actors",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_directors_DirectorId",
                table: "tbl_movie_directors",
                column: "DirectorId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_directors_MovieId",
                table: "tbl_movie_directors",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_genres_GenreId",
                table: "tbl_movie_genres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_genres_MovieId",
                table: "tbl_movie_genres",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_movie_images_MovieId",
                table: "tbl_movie_images",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_point_histories_BookingId",
                table: "tbl_point_histories",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_point_histories_UserId",
                table: "tbl_point_histories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_seat_log_SeatId",
                table: "tbl_seat_log",
                column: "SeatId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_seat_log_ShowTimeId",
                table: "tbl_seat_log",
                column: "ShowTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_seat_log_UserId",
                table: "tbl_seat_log",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_seats_RoomId",
                table: "tbl_seats",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_show_times_MovieId",
                table: "tbl_show_times",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_show_times_RoomId",
                table: "tbl_show_times",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tickets_BookingId",
                table: "tbl_tickets",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_tickets_ShowTimeId",
                table: "tbl_tickets",
                column: "ShowTimeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_transaction_BookingId",
                table: "tbl_transaction",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_promotions_PromotionId",
                table: "tbl_user_promotions",
                column: "PromotionId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_user_promotions_UserId",
                table: "tbl_user_promotions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "tbl_users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "tbl_users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConcessionItems");

            migrationBuilder.DropTable(
                name: "tbl_booking_details");

            migrationBuilder.DropTable(
                name: "tbl_employees");

            migrationBuilder.DropTable(
                name: "tbl_movie_actors");

            migrationBuilder.DropTable(
                name: "tbl_movie_directors");

            migrationBuilder.DropTable(
                name: "tbl_movie_genres");

            migrationBuilder.DropTable(
                name: "tbl_movie_images");

            migrationBuilder.DropTable(
                name: "tbl_point_histories");

            migrationBuilder.DropTable(
                name: "tbl_seat_log");

            migrationBuilder.DropTable(
                name: "tbl_tickets");

            migrationBuilder.DropTable(
                name: "tbl_transaction");

            migrationBuilder.DropTable(
                name: "tbl_user_promotions");

            migrationBuilder.DropTable(
                name: "tbl_actors");

            migrationBuilder.DropTable(
                name: "tbl_directors");

            migrationBuilder.DropTable(
                name: "tbl_genres");

            migrationBuilder.DropTable(
                name: "tbl_seats");

            migrationBuilder.DropTable(
                name: "tbl_bookings");

            migrationBuilder.DropTable(
                name: "tbl_promotions");

            migrationBuilder.DropTable(
                name: "tbl_show_times");

            migrationBuilder.DropTable(
                name: "tbl_users");

            migrationBuilder.DropTable(
                name: "tbl_cinema_rooms");

            migrationBuilder.DropTable(
                name: "tbl_movies");
        }
    }
}
