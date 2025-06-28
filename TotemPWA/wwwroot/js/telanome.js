const nomeInput = document.querySelector('.nome');

const keyboard = new SimpleKeyboard.default({
  onChange: input => onChange(input),
  onKeyPress: button => onKeyPress(button),
  theme: "hg-theme-default myTheme1",
  layout: {
    default: [
      "q w e r t y u i o p",
      "a s d f g h j k l",
      "z x c v b n m",
      "{space} {bksp}"
    ]
  },
  display: {
    "{bksp}": "⌫",
    "{space}": "Espaço"
  }
});

// function onChange(input) {
//   nomeInput.value = input;
//   keyboard.setInput(input);
// }

function onKeyPress(button) {
  if (button === "{shift}" || button === "{lock}") handleShift();
}

function handleShift() {
  const currentLayout = keyboard.options.layoutName;
  const shiftToggle = currentLayout === "default" ? "shift" : "default";
  keyboard.setOptions({ layoutName: shiftToggle });
}

// nomeInput.addEventListener("focus", () => {
//   document.querySelector(".simple-keyboard").style.display = "block";
// });

// document.addEventListener("click", (e) => {
//   if (!e.target.classList.contains("nome") && 
//       !e.target.closest(".simple-keyboard")) {
//     document.querySelector(".simple-keyboard").style.display = "none";
//   }
// });
