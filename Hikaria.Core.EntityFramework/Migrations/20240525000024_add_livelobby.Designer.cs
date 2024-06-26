﻿// <auto-generated />
using System;
using Hikaria.Core.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hikaria.Core.EntityFramework.Migrations
{
    [DbContext(typeof(GTFODbContext))]
    [Migration("20240525000024_add_livelobby")]
    partial class add_livelobby
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Hikaria.Core.Entities.BannedPlayer", b =>
                {
                    b.Property<long>("SteamID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("DateBanned")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("SteamID");

                    b.ToTable("BannedPlayers");
                });

            modelBuilder.Entity("Hikaria.Core.Entities.LiveLobby", b =>
                {
                    b.Property<long>("LobbyID")
                        .HasColumnType("bigint");

                    b.Property<DateTime>("ExpirationTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("LobbyName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.HasKey("LobbyID");

                    b.ToTable("LiveLobbies", (string)null);
                });

            modelBuilder.Entity("Hikaria.Core.Entities.SteamUser", b =>
                {
                    b.Property<long>("SteamID")
                        .HasColumnType("bigint");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Privilege")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(max)")
                        .HasDefaultValue("None");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .ValueGeneratedOnAdd()
                        .HasColumnType("nvarchar(max)")
                        .HasDefaultValue("");

                    b.HasKey("SteamID");

                    b.ToTable("SteamUsers");
                });

            modelBuilder.Entity("Hikaria.Core.Entities.LiveLobby", b =>
                {
                    b.OwnsOne("Hikaria.Core.Entities.DetailedLobbyInfo", "DetailedInfo", b1 =>
                        {
                            b1.Property<long>("LiveLobbyLobbyID")
                                .HasColumnType("bigint");

                            b1.Property<string>("Expedition")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)");

                            b1.Property<string>("ExpeditionName")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)");

                            b1.Property<decimal>("HostSteamID")
                                .HasColumnType("decimal(20,0)");

                            b1.Property<bool>("IsPlayingModded")
                                .HasColumnType("bit");

                            b1.Property<int>("MaxPlayerSlots")
                                .HasColumnType("int");

                            b1.Property<int>("OpenSlots")
                                .HasColumnType("int");

                            b1.Property<string>("RegionName")
                                .IsRequired()
                                .HasMaxLength(100)
                                .HasColumnType("nvarchar(100)");

                            b1.Property<int>("Revision")
                                .HasColumnType("int");

                            b1.Property<string>("SteamIDsInLobby")
                                .IsRequired()
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("LiveLobbyLobbyID");

                            b1.ToTable("LiveLobbies");

                            b1.WithOwner()
                                .HasForeignKey("LiveLobbyLobbyID");
                        });

                    b.OwnsOne("Hikaria.Core.Entities.LobbyPrivacySettings", "PrivacySettings", b1 =>
                        {
                            b1.Property<long>("LiveLobbyLobbyID")
                                .HasColumnType("bigint");

                            b1.Property<bool>("HasPassword")
                                .HasColumnType("bit");

                            b1.Property<int>("Privacy")
                                .HasColumnType("int");

                            b1.HasKey("LiveLobbyLobbyID");

                            b1.ToTable("LiveLobbies");

                            b1.WithOwner()
                                .HasForeignKey("LiveLobbyLobbyID");
                        });

                    b.OwnsOne("Hikaria.Core.Entities.LobbyStatusInfo", "StatusInfo", b1 =>
                        {
                            b1.Property<long>("LiveLobbyLobbyID")
                                .HasColumnType("bigint");

                            b1.Property<string>("StatusInfo")
                                .IsRequired()
                                .HasMaxLength(500)
                                .HasColumnType("nvarchar(500)");

                            b1.HasKey("LiveLobbyLobbyID");

                            b1.ToTable("LiveLobbies");

                            b1.WithOwner()
                                .HasForeignKey("LiveLobbyLobbyID");
                        });

                    b.Navigation("DetailedInfo")
                        .IsRequired();

                    b.Navigation("PrivacySettings")
                        .IsRequired();

                    b.Navigation("StatusInfo")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
