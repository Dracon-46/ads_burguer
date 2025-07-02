const nomeInput = document.querySelector('.nome');
const continueButton = document.querySelector('.btn-fim');
const nameError = document.getElementById('nameError');

const keyboard = new SimpleKeyboard.default({
  onChange: input => onChange(input),
  onKeyPress: button => onKeyPress(button),
  theme: "hg-theme-default myTheme1",
  layout: {
    default: [
      "q w e r t y u i o p",
      "a s d f g h j k l",
      "{shift} z x c v b n m {bksp}", // Adicionado {shift} aqui
      "{space}"
    ],
    shift: [
      "Q W E R T Y U I O P",
      "A S D F G H J K L",
      "{shift} Z X C V B N M {bksp}", // Adicionado {shift} aqui
      "{space}"
    ]
  },
  display: {
    "{bksp}": "⌫",
    "{space}": "Espaço",
    "{shift}": "⇧" // Ícone para shift
  }
});

function validateName(name) {
  // Permite apenas letras (maiúsculas e minúsculas) e espaços
  const regex = /^[A-Za-z\s]*$/;
  if (!regex.test(name)) {
    nameError.textContent = "Nome não pode conter números ou símbolos.";
    return false;
  }
  // Nome não pode ser vazio ou apenas espaços
  if (name.trim().length === 0) {
    nameError.textContent = "Nome não pode ser vazio.";
    return false;
  }
  nameError.textContent = ""; // Limpa o erro
  return true;
}

function onChange(input) {
  // Filtra a entrada para permitir apenas letras e espaços
  const filteredInput = input.replace(/[^A-Za-z\s]/g, '');
  nomeInput.value = filteredInput;
  keyboard.setInput(filteredInput);

  // Valida e habilita/desabilita o botão
  if (validateName(filteredInput)) {
    continueButton.disabled = false;
  } else {
    continueButton.disabled = true;
  }
}

function onKeyPress(button) {
  if (button === "{shift}" || button === "{lock}") handleShift();
}

function handleShift() {
  const currentLayout = keyboard.options.layoutName;
  const shiftToggle = currentLayout === "default" ? "shift" : "default";
  keyboard.setOptions({ layoutName: shiftToggle });
}

keyboard.setInput(nomeInput.value); // Apenas sincroniza, sem alterar display


nomeInput.addEventListener("input", function() {
  onChange(this.value); // Garante que a validação ocorra ao digitar diretamente também
});

// Event listener para o botão de continuar (agora sem o <a> direto)
continueButton.addEventListener("click", function(event) {
  if (!this.disabled) {
    // Se o botão não estiver desabilitado, navega para a próxima página
    window.location.href = 'SelecionarPedido';
  } else {
    // Impede a navegação se o botão estiver desabilitado
    event.preventDefault(); 
  }
});

// Estado inicial
continueButton.disabled = true;
nameError.textContent = "";