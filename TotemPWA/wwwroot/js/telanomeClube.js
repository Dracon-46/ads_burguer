
// ===========================
// telanomeClube.js - Versão melhorada
// ===========================

const nomeInput = document.querySelector('.nome');
const continueButton = document.querySelector('.btn-fim');
const nameError = document.getElementById('nameError');
const isEmployeeCheckbox = document.getElementById('isEmployee');
const employeeTypeSelect = document.getElementById('employeeType');

// Configuração do teclado virtual para nome
const nameKeyboard = new SimpleKeyboard.default({
  onChange: input => onNameChange(input),
  onKeyPress: button => onNameKeyPress(button),
  theme: "hg-theme-default myTheme1",
  layout: {
    default: [
      "q w e r t y u i o p",
      "a s d f g h j k l",
      "{shift} z x c v b n m {bksp}",
      "{space}"
    ],
    shift: [
      "Q W E R T Y U I O P",
      "A S D F G H J K L",
      "{shift} Z X C V B N M {bksp}",
      "{space}"
    ]
  },
  display: {
    "{bksp}": "⌫",
    "{space}": "Espaço",
    "{shift}": "⇧"
  }
});

// Validação do nome
function validateName(name) {
  const regex = /^[A-Za-z\s]*$/;
  if (!regex.test(name)) {
    nameError.textContent = "Nome não pode conter números ou símbolos.";
    return false;
  }
  if (name.trim().length === 0) {
    nameError.textContent = "Nome não pode ser vazio.";
    return false;
  }
  nameError.textContent = "";
  return true;
}

// Handlers do teclado virtual para nome
function onNameChange(input) {
  const filteredInput = input.replace(/[^A-Za-z\s]/g, '');
  nomeInput.value = filteredInput;
  nameKeyboard.setInput(filteredInput);
  updateContinueButton();
}

function onNameKeyPress(button) {
  if (button === "{shift}" || button === "{lock}") handleShift();
}

function handleShift() {
  const currentLayout = nameKeyboard.options.layoutName;
  const shiftToggle = currentLayout === "default" ? "shift" : "default";
  nameKeyboard.setOptions({ layoutName: shiftToggle });
}

// Atualizar estado do botão continuar
function updateContinueButton() {
  const isValidName = validateName(nomeInput.value);
  continueButton.disabled = !isValidName;
}

// Event listeners para nome
nomeInput.addEventListener("input", function() {
  onNameChange(this.value);
});

// Toggle para funcionário
if (isEmployeeCheckbox) {
  isEmployeeCheckbox.addEventListener("change", function() {
    employeeTypeSelect.style.display = this.checked ? "block" : "none";
    if (!this.checked) {
      employeeTypeSelect.value = "";
    }
  });
}

// Event listener para o botão continuar
continueButton.addEventListener("click", async function(event) {
  event.preventDefault();

  if (!this.disabled) {
    const clientName = nomeInput.value.trim();
    const storedCpf = sessionStorage.getItem('newClientCpf');
    const isEmployee = isEmployeeCheckbox ? isEmployeeCheckbox.checked : false;
    const employeeType = isEmployee ? employeeTypeSelect.value : null;

    if (!storedCpf) {
      nameError.textContent = "Erro: CPF não encontrado para registro.";
      return;
    }

    try {
      // Registrar cliente
      const clientResponse = await fetch('/Admin/Client/RegisterNewClient', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ 
          name: clientName, 
          cpf: storedCpf,
          isEmployee: isEmployee,
          employeeType: employeeType
        })
      });

      const clientData = await clientResponse.json();

      if (clientData.success) {
        // Armazenar informações do usuário
        sessionStorage.setItem('currentUser', JSON.stringify({
          clientId: clientData.clientId,
          name: clientName,
          cpf: storedCpf,
          isEmployee: isEmployee,
          employeeType: employeeType
        }));

        // Limpar CPF temporário
        sessionStorage.removeItem('newClientCpf');

        // Redirecionar para próxima tela
        window.location.href = 'SelecionarPedido';
      } else {
        nameError.textContent = clientData.message || "Erro ao cadastrar cliente.";
      }
    } catch (error) {
      console.error("Erro ao cadastrar cliente:", error);
      nameError.textContent = "Ocorreu um erro inesperado ao cadastrar.";
    }
  }
});

// Estado inicial
continueButton.disabled = true;
nameError.textContent = "";
nameKeyboard.setInput(nomeInput.value);