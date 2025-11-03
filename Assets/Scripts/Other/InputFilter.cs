using TMPro;
using UnityEngine;

public class InputFilter : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private int maxLength = 20; // set default or tweak in Inspector

    void Start()
    {
        inputField.characterLimit = maxLength;
        inputField.onValidateInput += ValidateChar;
    }

    private char ValidateChar(string text, int charIndex, char addedChar)
    {
        // Allow only letters, digits, underscore, and space
        if (char.IsLetterOrDigit(addedChar) || addedChar == '_' || addedChar == ' ')
            return addedChar;

        return '\0'; // reject invalid char
    }
}
