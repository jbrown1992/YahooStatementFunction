using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YahooStatementFunction.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    StockId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ticker = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.StockId);
                });

            migrationBuilder.CreateTable(
                name: "CashFlows",
                columns: table => new
                {
                    CashFlowId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    FiscalDateEnding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OperatingCashFlow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CapitalExpenditure = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FreeCashFlow = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashFlows", x => x.CashFlowId);
                    table.ForeignKey(
                        name: "FK_CashFlows_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "StockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IncomeStatements",
                columns: table => new
                {
                    IncomeStatementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockId = table.Column<int>(type: "int", nullable: false),
                    FiscalDateEnding = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRevenue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetIncome = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BasicAverageShares = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncomeStatements", x => x.IncomeStatementId);
                    table.ForeignKey(
                        name: "FK_IncomeStatements_Stocks_StockId",
                        column: x => x.StockId,
                        principalTable: "Stocks",
                        principalColumn: "StockId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashFlows_StockId",
                table: "CashFlows",
                column: "StockId");

            migrationBuilder.CreateIndex(
                name: "IX_IncomeStatements_StockId",
                table: "IncomeStatements",
                column: "StockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashFlows");

            migrationBuilder.DropTable(
                name: "IncomeStatements");

            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
