# GitHub Copilot Instructions for Dynamic Economy (Continued) RimWorld Mod

## Mod Overview and Purpose

The Dynamic Economy (Continued) mod is an update and continuation of the Saloids mod, enhancing the economic systems within the RimWorld game. The mod introduces several key features that aim to make the economic interactions more dynamic and engaging, including local prices, brain mining, and adaptive traders.

### Key Features and Systems

1. **Dynamic Prices:**
   - Trade caravans have changing prices based on your purchases and sales. For example, selling sculptures for a high price will cause the traders to offer lower prices for them in the future.
   - Prices will fluctuate depending on the resources and items bought and sold by the player.
   - Price multipliers will return to normal over time, so sending your own caravans can help mitigate high prices.

2. **Trading with Settlements:**
   - Separate price multipliers for settlements and trader caravans allow for more strategic trading.
   - Players can use the comms console to gather information about nearby settlements' current prices, enabling better negotiation opportunities.

3. **Trade Ships:**
   - Orbital traders with randomized prices, configurable through mod options.
   - These traders offer a different pricing model, providing variety in the market.

4. **Brain Mining:**
   - Introduction of a new source of income through the use of a psicoin miner implant.
   - Players can use brain mining to generate psicoin chips, though the price of psicoin is random and can fluctuate.

5. **Configuration Options:**
   - The mod provides options to configure price multipliers, growth, and drop rates for dynamic pricing.

6. **Compatibility and Error Reporting:**
   - The mod is designed to be savegame compatible and offers a standalone version without brain mining.
   - Errors and issues should be reported using the provided Discord channel.

## Coding Patterns and Conventions

- **Harmony Patching:** Utilizes Harmony for patching vanilla RimWorld methods to implement custom economic behaviors.
- **XML Integration:** Extensive use of XML files to define new economic events, jobs, hediffs, recipes, and other game elements.
- **Settings Management:** Provides a settings window to configure various aspects of the mod, including price modifiers and brain mining settings.
- **Error Handling:** Includes error logging and checks to ensure compatibility and stability across different game versions and other mods.

## XML Integration

The mod integrates with RimWorld through several XML files located in the `1.6/Defs` directory. Key XML files include:

- **Hediffs.xml:** Defines new hediffs related to brain mining.
- **Jobs.xml:** Adds new job definitions for brain mining activities.
- **PriceModifiers.xml:** Contains definitions for dynamic price modifiers.
- **QuestScripts.xml:** Defines quest scripts that can affect economic events.
- **Things.xml:** Includes new thing definitions for items related to brain mining.

## Harmony Patching

Harmony is used extensively in the mod to patch vanilla RimWorld methods to introduce new economic behaviors. Key patched methods include:

- **Trade Caravan Price Calculation:** Patches methods to adjust prices based on player purchases and sales.
- **Settlement Trade Price Adjustments:** Patches methods to adjust prices based on interactions with nearby settlements.
- **Brain Mining Logic:** Patches methods to implement the brain mining system and psicoin generation.

## Suggestions for Copilot

1. **Start with XML Definitions:**
   - When working with new game elements like hediffs, jobs, or recipes, start by defining them in the appropriate XML files.

2. **Use Harmony Patches:**
   - For implementing custom behaviors, use Harmony patches to modify existing RimWorld methods.

3. **Follow Coding Conventions:**
   - Ensure that your code follows the established coding patterns and conventions used in the mod, such as error handling and settings management.

4. **Test and Debug:**
   - After implementing new features or patches, thoroughly test them to ensure compatibility and stability.

5. **Document Your Changes:**
   - Add comments and documentation to your code to explain the purpose and functionality of your changes.

By following these guidelines, you can contribute effectively to the Dynamic Economy (Continued) mod, enhancing its features and improving the overall player experience in RimWorld.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

