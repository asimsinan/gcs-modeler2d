angular.module("mainfuzzy")
    .constant("enums", {
        Variable: { 'Resistivity': 1, 'Resistance': 2, 'Saturation': 3 },
        VariableList: [
            { Text: 'Resistivity', Value: 1 },
            { Text: 'Resistance', Value: 2 },
            { Text: 'Saturation', Value: 3 },
        ],
        Resistance: { 'VeryLoose': 1, 'Loose': 2, 'Medium': 3, 'Hard': 4, 'Rock': 5 },
        ResistanceList: [
            { Text: 'Very Loose', Value: 1 },
            { Text: 'Loose', Value: 2 },
            { Text: 'Medium', Value: 3 },
            { Text: 'Hard', Value: 4 },
            { Text: 'Rock', Value: 5 },
        ],
        Saturation: { 'WaterSaturated': 1, 'WaterAndGasSaturated': 2, 'GasSaturated': 3 },
        SaturationList: [
            { Text: 'Water Saturated', Value: 1 },
            { Text: 'Water And Gas Saturated', Value: 2 },
            { Text: 'Gas Saturated', Value: 3 },
        ],
        Equality: { 'Equals': 1, 'NotEquals': 2 },
        EqualityList: [
            { Text: 'Equals (==)', Value: 1 },
            { Text: 'Not Equals (<>)', Value: 2 },
        ],
        Operator: { 'And': 1, 'Or': 2, 'None' : 3 },
        OperatorList: [
            { Text: 'And (&&)', Value: 1 },
            { Text: 'Or (||)', Value: 2 },
        ],
    });