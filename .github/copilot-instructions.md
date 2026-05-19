# GitHub Copilot Instructions for Dynamic Economy (Continued)

## Mod Overview and Purpose
**Dynamic Economy (Continued)** is a mod designed to enhance the trading and economic experience in RimWorld. By introducing fluctuating local prices, brain mining, and adaptive traders, it aims to create a more dynamic and engaging trading system. The mod expands on the original concept by Saloid, with updates and new features to address player feedback and enrich gameplay.

## Key Features and Systems
- **Trade Caravan Changes**: The mod introduces variable pricing based on player interactions with caravans. Prices change due to supply and demand, influencing what caravans bring next time.
- **Trading with Settlements**: Encourages sending player caravans to settlements that have their own unique price multipliers. Information gathering through comms consoles allows players to make informed trading decisions.
- **Orbital Trade Ships**: Orbital traders have dynamically randomized prices, providing varying trade opportunities.
- **Brain Mining**: Players can install psicoin miner implants in pawns to generate coins from their brain activity. The market price of these psicoins fluctuates randomly.
- **Price Multipliers**: Buying and selling price multipliers act independently. Growth occurs linearly, while the drop is exponential.

## Coding Patterns and Conventions
- **Class Definitions**: Follow a naming pattern starting with class type and purpose (e.g., `Building_CommsConsole_GetFloatMenuOptions`).
- **Method Naming**: Follow proper C# conventions with camelCase for private methods and PascalCase for public methods.
- **Modular Design**: Each feature is encapsulated within its specific class, following a single responsibility principle.
- **XML Handling**: `LoadDataFromXmlCustom` methods handle custom XML loading for data classes.

## XML Integration
- XML files are used to define various price multipliers and settings within the mod. 
- Classes like `BaseThingPriceMultipilerInfo` and `BaseCategoryPriceMultipilerInfo` have custom XML loading logic to integrate game data with XML definitions.

## Harmony Patching
- **Harmony**: Utilized for runtime method modification without altering the original game code.
- **Patching Strategy**: Ensure that methods from core game functions are carefully patched to enhance or modify behavior pertinent to dynamic pricing and trade interactions.

## Suggestions for Copilot
To assist with the development and extension of Dynamic Economy, consider the following suggestions:
1. **Predictive Refactoring**: Where code may benefit from cleanup or restructuring, suggest alternative patterns while maintaining functionality.
2. **Automated XML Suggestions**: Provide suggestions for XML data structures based on detected patterns and existing files, helping to keep XML consistent and adhering to existing conventions.
3. **Harmony Patch Examples**: Copilot should recognize when a method might benefit from patching and suggest potential pre/postfix logic relevant to trading operations.
4. **Debugging Support**: Suggest test methods or instrumentation for monitoring changes in price vectors, especially when experimenting with new economic models.
5. **Integration Testing**: Propose strategies for testing compatibility with base game features and other mods to ensure stability and compatibility.

By following these guidelines, Copilot can effectively aid the ongoing development and extension of the Dynamic Economy mod, ensuring both stability and new feature integration are accomplished smoothly.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.
