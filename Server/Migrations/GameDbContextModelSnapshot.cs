﻿// <auto-generated />
using System;
using MarketMakingGame.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MarketMakingGame.Server.Migrations
{
    [DbContext(typeof(GameDbContext))]
    partial class GameDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.9");

            modelBuilder.Entity("MarketMakingGame.Server.Models.GameState", b =>
                {
                    b.Property<int>("GameStateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GameId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsFinished")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerId")
                        .HasColumnType("TEXT");

                    b.HasKey("GameStateId");

                    b.HasIndex("PlayerId");

                    b.HasIndex("GameId", "PlayerId")
                        .IsUnique();

                    b.ToTable("GameStates");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.PlayerState", b =>
                {
                    b.Property<int>("PlayerStateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double?>("CurrentAsk")
                        .HasColumnType("REAL");

                    b.Property<double?>("CurrentBid")
                        .HasColumnType("REAL");

                    b.Property<int>("GameStateId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerCardCardId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerId")
                        .HasColumnType("TEXT");

                    b.HasKey("PlayerStateId");

                    b.HasIndex("PlayerCardCardId");

                    b.HasIndex("PlayerId");

                    b.HasIndex("GameStateId", "PlayerId")
                        .IsUnique();

                    b.ToTable("PlayerStates");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.RoundState", b =>
                {
                    b.Property<int>("RoundStateId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CommunityCardCardId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameStateId")
                        .HasColumnType("INTEGER");

                    b.HasKey("RoundStateId");

                    b.HasIndex("CommunityCardCardId");

                    b.HasIndex("GameStateId");

                    b.ToTable("RoundStates");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.Trade", b =>
                {
                    b.Property<int>("TradeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("GameStateId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("InitiatingPlayerPlayerId")
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsBuy")
                        .HasColumnType("INTEGER");

                    b.Property<string>("TargetPlayerPlayerId")
                        .HasColumnType("TEXT");

                    b.Property<double>("TradePrice")
                        .HasColumnType("REAL");

                    b.HasKey("TradeId");

                    b.HasIndex("GameStateId");

                    b.HasIndex("InitiatingPlayerPlayerId");

                    b.HasIndex("TargetPlayerPlayerId");

                    b.ToTable("Trades");
                });

            modelBuilder.Entity("MarketMakingGame.Shared.Models.Card", b =>
                {
                    b.Property<int>("CardId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CardDescription")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("CardImageUrl")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("CardValue")
                        .HasColumnType("REAL");

                    b.HasKey("CardId");

                    b.ToTable("Cards");

                    b.HasData(
                        new
                        {
                            CardId = 1,
                            CardDescription = "Ace of Diamonds",
                            CardImageUrl = "/images/cards/ace_of_diamonds.svg",
                            CardValue = -1.0
                        },
                        new
                        {
                            CardId = 2,
                            CardDescription = "2 of Diamonds",
                            CardImageUrl = "/images/cards/2_of_diamonds.svg",
                            CardValue = -2.0
                        },
                        new
                        {
                            CardId = 3,
                            CardDescription = "3 of Diamonds",
                            CardImageUrl = "/images/cards/3_of_diamonds.svg",
                            CardValue = -3.0
                        },
                        new
                        {
                            CardId = 4,
                            CardDescription = "4 of Diamonds",
                            CardImageUrl = "/images/cards/4_of_diamonds.svg",
                            CardValue = -4.0
                        },
                        new
                        {
                            CardId = 5,
                            CardDescription = "5 of Diamonds",
                            CardImageUrl = "/images/cards/5_of_diamonds.svg",
                            CardValue = -5.0
                        },
                        new
                        {
                            CardId = 6,
                            CardDescription = "6 of Diamonds",
                            CardImageUrl = "/images/cards/6_of_diamonds.svg",
                            CardValue = -6.0
                        },
                        new
                        {
                            CardId = 7,
                            CardDescription = "7 of Diamonds",
                            CardImageUrl = "/images/cards/7_of_diamonds.svg",
                            CardValue = -7.0
                        },
                        new
                        {
                            CardId = 8,
                            CardDescription = "8 of Diamonds",
                            CardImageUrl = "/images/cards/8_of_diamonds.svg",
                            CardValue = -8.0
                        },
                        new
                        {
                            CardId = 9,
                            CardDescription = "9 of Diamonds",
                            CardImageUrl = "/images/cards/9_of_diamonds.svg",
                            CardValue = -9.0
                        },
                        new
                        {
                            CardId = 10,
                            CardDescription = "10 of Diamonds",
                            CardImageUrl = "/images/cards/10_of_diamonds.svg",
                            CardValue = -10.0
                        },
                        new
                        {
                            CardId = 11,
                            CardDescription = "Jack of Diamonds",
                            CardImageUrl = "/images/cards/jack_of_diamonds.svg",
                            CardValue = -11.0
                        },
                        new
                        {
                            CardId = 12,
                            CardDescription = "Queen of Diamonds",
                            CardImageUrl = "/images/cards/queen_of_diamonds.svg",
                            CardValue = -12.0
                        },
                        new
                        {
                            CardId = 13,
                            CardDescription = "King of Diamonds",
                            CardImageUrl = "/images/cards/king_of_diamonds.svg",
                            CardValue = -13.0
                        },
                        new
                        {
                            CardId = 14,
                            CardDescription = "Ace of Hearts",
                            CardImageUrl = "/images/cards/ace_of_hearts.svg",
                            CardValue = -1.0
                        },
                        new
                        {
                            CardId = 15,
                            CardDescription = "2 of Hearts",
                            CardImageUrl = "/images/cards/2_of_hearts.svg",
                            CardValue = -2.0
                        },
                        new
                        {
                            CardId = 16,
                            CardDescription = "3 of Hearts",
                            CardImageUrl = "/images/cards/3_of_hearts.svg",
                            CardValue = -3.0
                        },
                        new
                        {
                            CardId = 17,
                            CardDescription = "4 of Hearts",
                            CardImageUrl = "/images/cards/4_of_hearts.svg",
                            CardValue = -4.0
                        },
                        new
                        {
                            CardId = 18,
                            CardDescription = "5 of Hearts",
                            CardImageUrl = "/images/cards/5_of_hearts.svg",
                            CardValue = -5.0
                        },
                        new
                        {
                            CardId = 19,
                            CardDescription = "6 of Hearts",
                            CardImageUrl = "/images/cards/6_of_hearts.svg",
                            CardValue = -6.0
                        },
                        new
                        {
                            CardId = 20,
                            CardDescription = "7 of Hearts",
                            CardImageUrl = "/images/cards/7_of_hearts.svg",
                            CardValue = -7.0
                        },
                        new
                        {
                            CardId = 21,
                            CardDescription = "8 of Hearts",
                            CardImageUrl = "/images/cards/8_of_hearts.svg",
                            CardValue = -8.0
                        },
                        new
                        {
                            CardId = 22,
                            CardDescription = "9 of Hearts",
                            CardImageUrl = "/images/cards/9_of_hearts.svg",
                            CardValue = -9.0
                        },
                        new
                        {
                            CardId = 23,
                            CardDescription = "10 of Hearts",
                            CardImageUrl = "/images/cards/10_of_hearts.svg",
                            CardValue = -10.0
                        },
                        new
                        {
                            CardId = 24,
                            CardDescription = "Jack of Hearts",
                            CardImageUrl = "/images/cards/jack_of_hearts.svg",
                            CardValue = -11.0
                        },
                        new
                        {
                            CardId = 25,
                            CardDescription = "Queen of Hearts",
                            CardImageUrl = "/images/cards/queen_of_hearts.svg",
                            CardValue = -12.0
                        },
                        new
                        {
                            CardId = 26,
                            CardDescription = "King of Hearts",
                            CardImageUrl = "/images/cards/king_of_hearts.svg",
                            CardValue = -13.0
                        },
                        new
                        {
                            CardId = 27,
                            CardDescription = "Ace of Spades",
                            CardImageUrl = "/images/cards/ace_of_spades.svg",
                            CardValue = 1.0
                        },
                        new
                        {
                            CardId = 28,
                            CardDescription = "2 of Spades",
                            CardImageUrl = "/images/cards/2_of_spades.svg",
                            CardValue = 2.0
                        },
                        new
                        {
                            CardId = 29,
                            CardDescription = "3 of Spades",
                            CardImageUrl = "/images/cards/3_of_spades.svg",
                            CardValue = 3.0
                        },
                        new
                        {
                            CardId = 30,
                            CardDescription = "4 of Spades",
                            CardImageUrl = "/images/cards/4_of_spades.svg",
                            CardValue = 4.0
                        },
                        new
                        {
                            CardId = 31,
                            CardDescription = "5 of Spades",
                            CardImageUrl = "/images/cards/5_of_spades.svg",
                            CardValue = 5.0
                        },
                        new
                        {
                            CardId = 32,
                            CardDescription = "6 of Spades",
                            CardImageUrl = "/images/cards/6_of_spades.svg",
                            CardValue = 6.0
                        },
                        new
                        {
                            CardId = 33,
                            CardDescription = "7 of Spades",
                            CardImageUrl = "/images/cards/7_of_spades.svg",
                            CardValue = 7.0
                        },
                        new
                        {
                            CardId = 34,
                            CardDescription = "8 of Spades",
                            CardImageUrl = "/images/cards/8_of_spades.svg",
                            CardValue = 8.0
                        },
                        new
                        {
                            CardId = 35,
                            CardDescription = "9 of Spades",
                            CardImageUrl = "/images/cards/9_of_spades.svg",
                            CardValue = 9.0
                        },
                        new
                        {
                            CardId = 36,
                            CardDescription = "10 of Spades",
                            CardImageUrl = "/images/cards/10_of_spades.svg",
                            CardValue = 10.0
                        },
                        new
                        {
                            CardId = 37,
                            CardDescription = "Jack of Spades",
                            CardImageUrl = "/images/cards/jack_of_spades.svg",
                            CardValue = 11.0
                        },
                        new
                        {
                            CardId = 38,
                            CardDescription = "Queen of Spades",
                            CardImageUrl = "/images/cards/queen_of_spades.svg",
                            CardValue = 12.0
                        },
                        new
                        {
                            CardId = 39,
                            CardDescription = "King of Spades",
                            CardImageUrl = "/images/cards/king_of_spades.svg",
                            CardValue = 13.0
                        },
                        new
                        {
                            CardId = 40,
                            CardDescription = "Ace of Clubs",
                            CardImageUrl = "/images/cards/ace_of_clubs.svg",
                            CardValue = 1.0
                        },
                        new
                        {
                            CardId = 41,
                            CardDescription = "2 of Clubs",
                            CardImageUrl = "/images/cards/2_of_clubs.svg",
                            CardValue = 2.0
                        },
                        new
                        {
                            CardId = 42,
                            CardDescription = "3 of Clubs",
                            CardImageUrl = "/images/cards/3_of_clubs.svg",
                            CardValue = 3.0
                        },
                        new
                        {
                            CardId = 43,
                            CardDescription = "4 of Clubs",
                            CardImageUrl = "/images/cards/4_of_clubs.svg",
                            CardValue = 4.0
                        },
                        new
                        {
                            CardId = 44,
                            CardDescription = "5 of Clubs",
                            CardImageUrl = "/images/cards/5_of_clubs.svg",
                            CardValue = 5.0
                        },
                        new
                        {
                            CardId = 45,
                            CardDescription = "6 of Clubs",
                            CardImageUrl = "/images/cards/6_of_clubs.svg",
                            CardValue = 6.0
                        },
                        new
                        {
                            CardId = 46,
                            CardDescription = "7 of Clubs",
                            CardImageUrl = "/images/cards/7_of_clubs.svg",
                            CardValue = 7.0
                        },
                        new
                        {
                            CardId = 47,
                            CardDescription = "8 of Clubs",
                            CardImageUrl = "/images/cards/8_of_clubs.svg",
                            CardValue = 8.0
                        },
                        new
                        {
                            CardId = 48,
                            CardDescription = "9 of Clubs",
                            CardImageUrl = "/images/cards/9_of_clubs.svg",
                            CardValue = 9.0
                        },
                        new
                        {
                            CardId = 49,
                            CardDescription = "10 of Clubs",
                            CardImageUrl = "/images/cards/10_of_clubs.svg",
                            CardValue = 10.0
                        },
                        new
                        {
                            CardId = 50,
                            CardDescription = "Jack of Clubs",
                            CardImageUrl = "/images/cards/jack_of_clubs.svg",
                            CardValue = 11.0
                        },
                        new
                        {
                            CardId = 51,
                            CardDescription = "Queen of Clubs",
                            CardImageUrl = "/images/cards/queen_of_clubs.svg",
                            CardValue = 12.0
                        },
                        new
                        {
                            CardId = 52,
                            CardDescription = "King of Clubs",
                            CardImageUrl = "/images/cards/king_of_clubs.svg",
                            CardValue = 13.0
                        },
                        new
                        {
                            CardId = 53,
                            CardDescription = "Unopened",
                            CardImageUrl = "/images/cards/back_card_red.svg",
                            CardValue = 0.0
                        });
                });

            modelBuilder.Entity("MarketMakingGame.Shared.Models.Game", b =>
                {
                    b.Property<string>("GameId")
                        .HasColumnType("char")
                        .HasMaxLength(36);

                    b.Property<string>("GameName")
                        .IsRequired()
                        .HasColumnType("char")
                        .HasMaxLength(20);

                    b.Property<double?>("MaxQuoteWidth")
                        .HasColumnType("REAL");

                    b.Property<double?>("MinQuoteWidth")
                        .HasColumnType("REAL");

                    b.Property<int?>("NumberOfRounds")
                        .HasColumnType("INTEGER");

                    b.HasKey("GameId");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("MarketMakingGame.Shared.Models.Player", b =>
                {
                    b.Property<string>("PlayerId")
                        .HasColumnType("char")
                        .HasMaxLength(36);

                    b.Property<string>("AvatarSeed")
                        .IsRequired()
                        .HasColumnType("char")
                        .HasMaxLength(100);

                    b.Property<string>("DisplayName")
                        .IsRequired()
                        .HasColumnType("char")
                        .HasMaxLength(20);

                    b.HasKey("PlayerId");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.GameState", b =>
                {
                    b.HasOne("MarketMakingGame.Shared.Models.Game", "Game")
                        .WithMany()
                        .HasForeignKey("GameId");

                    b.HasOne("MarketMakingGame.Shared.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.PlayerState", b =>
                {
                    b.HasOne("MarketMakingGame.Server.Models.GameState", "GameState")
                        .WithMany("PlayerStates")
                        .HasForeignKey("GameStateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMakingGame.Shared.Models.Card", "PlayerCard")
                        .WithMany()
                        .HasForeignKey("PlayerCardCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMakingGame.Shared.Models.Player", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId");
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.RoundState", b =>
                {
                    b.HasOne("MarketMakingGame.Shared.Models.Card", "CommunityCard")
                        .WithMany()
                        .HasForeignKey("CommunityCardCardId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMakingGame.Server.Models.GameState", "GameState")
                        .WithMany("RoundStates")
                        .HasForeignKey("GameStateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("MarketMakingGame.Server.Models.Trade", b =>
                {
                    b.HasOne("MarketMakingGame.Server.Models.GameState", "GameState")
                        .WithMany("Trades")
                        .HasForeignKey("GameStateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("MarketMakingGame.Shared.Models.Player", "InitiatingPlayer")
                        .WithMany()
                        .HasForeignKey("InitiatingPlayerPlayerId");

                    b.HasOne("MarketMakingGame.Shared.Models.Player", "TargetPlayer")
                        .WithMany()
                        .HasForeignKey("TargetPlayerPlayerId");
                });
#pragma warning restore 612, 618
        }
    }
}
