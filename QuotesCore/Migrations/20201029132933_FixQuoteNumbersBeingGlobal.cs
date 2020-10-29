using Microsoft.EntityFrameworkCore.Migrations;

namespace QuotesCore.Migrations
{
    public partial class FixQuoteNumbersBeingGlobal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"SET CHARACTER SET utf8mb4, NAMES utf8mb4, collation_connection = 'utf8mb4_general_ci', lc_messages = 'en_US', time_zone = '+00:00';
                DROP PROCEDURE IF EXISTS FIXQUOTENUMBERS;
            ");
            migrationBuilder.Sql(@"
                CREATE PROCEDURE FIXQUOTENUMBERS()
                BEGIN
                    DECLARE COUNTER INT DEFAULT 0;
                    DECLARE cursor_GUILD_ID VARCHAR(36);
                    DECLARE guilds_done INT DEFAULT FALSE;
                    DECLARE cursor_i CURSOR FOR SELECT GuildId FROM Quotes GROUP BY GuildId;
                    DECLARE CONTINUE HANDLER FOR NOT FOUND SET guilds_done = TRUE;
                    OPEN cursor_i;
                    guild_loop: LOOP
                        FETCH cursor_i INTO cursor_GUILD_ID;
                        IF guilds_done THEN
                        LEAVE guild_loop;
                        END IF;

                        INNER_BLOCK: BEGIN
                            DECLARE cursor_QUOTE_ID VARCHAR(36);
                            DECLARE quotes_done INT DEFAULT FALSE;
                            DECLARE cursor_j CURSOR FOR SELECT Id FROM Quotes WHERE DeletedAt is NULL AND GuildId = cursor_GUILD_ID COLLATE utf8mb4_unicode_ci ORDER BY QuoteNumber ASC;
                            DECLARE CONTINUE HANDLER FOR NOT FOUND SET quotes_done = TRUE;

                            OPEN cursor_j;
                            quote_loop: LOOP
                                SELECT (SELECT (COUNTER + 1)) INTO COUNTER;
                                FETCH cursor_j INTO cursor_QUOTE_ID;
                                IF quotes_done THEN
                                    LEAVE quote_loop;
                                END IF;

                                UPDATE Quotes SET QuoteNumber = COUNTER WHERE Id = cursor_QUOTE_ID COLLATE utf8mb4_unicode_ci;
                            END LOOP quote_loop;
                            CLOSE cursor_j;
                            SELECT 0 INTO COUNTER;
                        END INNER_BLOCK;
                    END LOOP guild_loop;
                    CLOSE cursor_i;
                End;
            ");

            migrationBuilder.Sql(@"
                CALL FIXQUOTENUMBERS();
                DROP PROCEDURE IF EXISTS FIXQUOTENUMBERS;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @counter = 0;
                UPDATE Quotes
                    SET QuoteNumber = (SELECT @counter := @counter + 1)
                    WHERE DeletedAt is NULL
                    ORDER BY CreatedAt ASC;
            ");
        }
    }
}
