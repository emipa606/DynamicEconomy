# GitHub Copilot Instructions for RimWorld Mod: Dynamic Economy (Continued)

## Mod Overview and Purpose
Dynamic Economy (Continued), originally by saloid, is a RimWorld mod that introduces a dynamic trade system where prices adapt according to supply and demand, brain mining for psicoin production, and interactions with orbital traders. This mod aims to enrich the trading experience in RimWorld by implementing localized economy mechanics.

## Key Features and Systems
1. **Trade Caravan Changes**: Prices fluctuate based on your transactions with caravans. Frequent sales of specific goods will lower their purchase price over time, while buying in bulk leads to higher selling prices in the future.
   
2. **Trading with Settlements**: Each settlement has its own unique multiplier for buys and sells, encouraging players to embark on caravans for potentially better deals. Players can gather market intelligence via the comms console to spot short-term price changes.

3. **Trade Ships**: Orbital traders operate with randomized prices, representing different markets away from the RimWorld. This randomness offers opportunities for profit through strategic buying and selling.

4. **Brain Mining**: Enables the crafting of psicoin miner implants after bionics research, turning pawns into psicoin producers. Psicoin values are volatile, providing a fluctuating source of income.

5. **Price Multiplier Mechanics**: Growth is linear, and decrease in multipliers is exponential. Prices gradually normalize over time, and these parameters are configurable in the mod settings.

## Coding Patterns and Conventions
- **Consistent Naming**: Class, method, and variable names follow PascalCase and camelCase conventions, ensuring code readability and consistency.
- **Code Modularity**: Classes are designed to encapsulate specific functionalities (e.g., `ComplexPriceModifier` handles price dynamics) for maintainability.
- **Error Handling**: Logging is consistently used to capture exceptions and game state changes, aiding in debugging.

## XML Integration
- **Defs Usage**: XML files define game aspects such as `HediffDef`, `JobDef`, `DynamicEconomy.ConstantPriceModsDef`, etc., used to extend and modify RimWorld gameplay elements.
- **Custom XML Loading**: The method `LoadDataFromXmlCustom` in `BaseThingPriceMultipilerInfo` processes custom XML data entries, integrating seamlessly with existing game data structures.

## Harmony Patching
- **Patch Integration**: The mod utilizes Harmony (brrainz.harmony) for method patches, allowing dynamic alterations of game behaviors without modifying the original source code.
- **Patch Structure**: Patches are clearly documented and separated, with pre/postfix methods applied judiciously to modify game functions.

## Suggestions for Copilot
1. **Auto-Completion and Snippets**: Use Copilot to provide code snippets for XML data parsing and Harmony patches, following existing patterns in the mod.
   
2. **Refactoring Assistance**: Suggest improvements or refactorings, especially in complex methods like `ComplexPriceModifier.GetPriceMultipilerFor` to enhance performance and readability.

3. **Error Message Suggestions**: Recommend detailed error messages and logging improvements to handle potential exceptions and runtime errors more effectively.

4. **Configuration and Setting Management**: Offer code examples for extending and managing the mod's in-game settings through the `DESettings` class.

5. **XML Def Management**: Assist in generating XML templates that maintain consistency across newly added defs and existing ones.

Use these instructions to guide Copilot in generating relevant and cohesive code that aligns with the mod’s structure and mechanics.

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

