using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AikiDataBuilder.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SuspendedOn = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayableCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodTo = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayableCharges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Platforms",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platforms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlatformId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformUsages_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivableCharges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivableCharges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivableCharges_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CustomerId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PayableCharge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChargeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeType = table.Column<int>(type: "int", nullable: false),
                    BillingCycleType = table.Column<int>(type: "int", nullable: false),
                    PeriodFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPriceProrated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    IsProratable = table.Column<bool>(type: "bit", nullable: false),
                    PayableChargesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayableCharge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayableCharge_PayableCharges_PayableChargesId",
                        column: x => x.PayableChargesId,
                        principalTable: "PayableCharges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalizedNames",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Culture = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatformId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalizedNames", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalizedNames_Platforms_PlatformId",
                        column: x => x.PlatformId,
                        principalTable: "Platforms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MeterUsages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeterId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalQuantities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsumedQuantities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQuantities = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PlatformUsageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeterUsages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MeterUsages_PlatformUsages_PlatformUsageId",
                        column: x => x.PlatformUsageId,
                        principalTable: "PlatformUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlatformId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlatformUsageId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformDetails_PlatformUsages_PlatformUsageId",
                        column: x => x.PlatformUsageId,
                        principalTable: "PlatformUsages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceivableCharge",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChargeType = table.Column<int>(type: "int", nullable: false),
                    BillingCycleType = table.Column<int>(type: "int", nullable: false),
                    PeriodFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPriceProrated = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsProratable = table.Column<bool>(type: "bit", nullable: false),
                    ReceivableChargesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceivableCharge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceivableCharge_ReceivableCharges_ReceivableChargesId",
                        column: x => x.ReceivableChargesId,
                        principalTable: "ReceivableCharges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommitmentTerms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    TermEndDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommitmentTerms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommitmentTerms_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionFees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecurringFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SetupFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SubscriptionId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionFees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionFees_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Deductions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeductionType = table.Column<int>(type: "int", nullable: false),
                    Value = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayableChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deductions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deductions_PayableCharge_PayableChargeId",
                        column: x => x.PayableChargeId,
                        principalTable: "PayableCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UnitValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalValue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    PayableChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fees", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fees_PayableCharge_PayableChargeId",
                        column: x => x.PayableChargeId,
                        principalTable: "PayableCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodFrom = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayableChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_PayableCharge_PayableChargeId",
                        column: x => x.PayableChargeId,
                        principalTable: "PayableCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PayableChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tags_PayableCharge_PayableChargeId",
                        column: x => x.PayableChargeId,
                        principalTable: "PayableCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AppliedRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PayableChargeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Taxes_PayableCharge_PayableChargeId",
                        column: x => x.PayableChargeId,
                        principalTable: "PayableCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CommittedMinimalQuantities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommittedUntil = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommitmentTermId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommittedMinimalQuantities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommittedMinimalQuantities_CommitmentTerms_CommitmentTermId",
                        column: x => x.CommitmentTermId,
                        principalTable: "CommitmentTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RenewalConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RenewalDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommitmentTermId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RenewalConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RenewalConfigurations_CommitmentTerms_CommitmentTermId",
                        column: x => x.CommitmentTermId,
                        principalTable: "CommitmentTerms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommitmentTerms_SubscriptionId",
                table: "CommitmentTerms",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommittedMinimalQuantities_CommitmentTermId",
                table: "CommittedMinimalQuantities",
                column: "CommitmentTermId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_DisplayName",
                table: "Customers",
                column: "DisplayName");

            migrationBuilder.CreateIndex(
                name: "IX_Deductions_PayableChargeId",
                table: "Deductions",
                column: "PayableChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Fees_PayableChargeId",
                table: "Fees",
                column: "PayableChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Number",
                table: "Invoices",
                column: "Number");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PayableChargeId",
                table: "Invoices",
                column: "PayableChargeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LocalizedNames_PlatformId",
                table: "LocalizedNames",
                column: "PlatformId");

            migrationBuilder.CreateIndex(
                name: "IX_MeterUsages_PlatformUsageId",
                table: "MeterUsages",
                column: "PlatformUsageId");

            migrationBuilder.CreateIndex(
                name: "IX_PayableCharge_ChargeId",
                table: "PayableCharge",
                column: "ChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_PayableCharge_PayableChargesId",
                table: "PayableCharge",
                column: "PayableChargesId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDetails_PlatformUsageId",
                table: "PlatformDetails",
                column: "PlatformUsageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUsages_CustomerId",
                table: "PlatformUsages",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableCharge_ReceivableChargesId",
                table: "ReceivableCharge",
                column: "ReceivableChargesId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceivableCharges_CustomerId",
                table: "ReceivableCharges",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RenewalConfigurations_CommitmentTermId",
                table: "RenewalConfigurations",
                column: "CommitmentTermId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFees_SubscriptionId",
                table: "SubscriptionFees",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_CustomerId",
                table: "Subscriptions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_PayableChargeId",
                table: "Tags",
                column: "PayableChargeId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxes_PayableChargeId",
                table: "Taxes",
                column: "PayableChargeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommittedMinimalQuantities");

            migrationBuilder.DropTable(
                name: "Deductions");

            migrationBuilder.DropTable(
                name: "Fees");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "LocalizedNames");

            migrationBuilder.DropTable(
                name: "MeterUsages");

            migrationBuilder.DropTable(
                name: "PlatformDetails");

            migrationBuilder.DropTable(
                name: "ReceivableCharge");

            migrationBuilder.DropTable(
                name: "RenewalConfigurations");

            migrationBuilder.DropTable(
                name: "SubscriptionFees");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropTable(
                name: "Platforms");

            migrationBuilder.DropTable(
                name: "PlatformUsages");

            migrationBuilder.DropTable(
                name: "ReceivableCharges");

            migrationBuilder.DropTable(
                name: "CommitmentTerms");

            migrationBuilder.DropTable(
                name: "PayableCharge");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "PayableCharges");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
