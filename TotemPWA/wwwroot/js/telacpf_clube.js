const cpfInput = document.querySelector('.inputCPF');
const keyboard = new SimpleKeyboard.default({
  onChange: input => onChange(input),
  onKeyPress: button => onKeyPress(button),
  theme: "hg-theme-default myTheme1",
  layout: {
    default: [
      "1 2 3",
      "4 5 6",
      "7 8 9",
      "0 {bksp}"
    ]
  },
  display: {
    "{bksp}": "⌫"
  }
});

function formatCPF(value) {
  let cpf = value.replace(/\D/g, ''); // Remove não números
  if (cpf.length > 11) cpf = cpf.slice(0, 11); // Limita a 11 dígitos

  cpf = cpf
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2');

  return cpf;
}

function onChange(input) {
  const formatted = formatCPF(input);
  cpfInput.value = formatted;
  keyboard.setInput(formatted);
}

function onKeyPress(button) {
  if (button === "{shift}" || button === "{lock}") handleShift();
}

function handleShift() {
  const currentLayout = keyboard.options.layoutName;
  const shiftToggle = currentLayout === "default" ? "shift" : "default";

  keyboard.setOptions({
    layoutName: shiftToggle
  });
}

cpfInput.addEventListener("focus", () => {
  document.querySelector(".simple-keyboard").style.display = "block";
});

document.addEventListener("click", (event) => {
  if (!event.target.classList.contains("inputCPF") &&
      !event.target.closest(".simple-keyboard")) {
    document.querySelector(".simple-keyboard").style.display = "none";
  }
});

cpfInput.addEventListener("input", function () {
  const formatted = formatCPF(this.value);
  this.value = formatted;
  keyboard.setInput(formatted); // Sincroniza com o teclado virtual
});
