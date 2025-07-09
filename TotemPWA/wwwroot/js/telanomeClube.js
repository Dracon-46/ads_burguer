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
continueButton.addEventListener("click", async function(event) {
  event.preventDefault(); // Previne o envio padrão do formulário inicialmente

  if (!this.disabled) { // Se o botão não estiver desabilitado
    const clientName = nomeInput.value.trim(); // Obtém o nome digitado
    // Aqui, você enviaria o nome do cliente (e o CPF inserido anteriormente, talvez armazenado em localStorage ou sessão)
    // para um endpoint de backend para registro.

    // Exemplo de chamada de backend para registrar um novo cliente
    try {
      // Você precisará passar o CPF que foi inserido na tela anterior.
      // Para este exemplo, vamos assumir que você o armazena em localStorage após a verificação do CPF.
      const storedCpf = localStorage.getItem('newClientCpf'); // Recupera o CPF
      if (!storedCpf) {
        nameError.textContent = "Erro: CPF não encontrado para registro.";
        return;
      }

      const response = await fetch('/Admin/Client/RegisterNewClient', { // Novo endpoint para registro
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: clientName, cpf: storedCpf })
      });

      const data = await response.json();

      if (data.success) { // Se o cadastro for bem-sucedido
        window.location.href = 'SelecionarPedido'; // Redireciona para a próxima página
      } else {
        nameError.textContent = data.message || "Erro ao cadastrar cliente."; // Exibe mensagem de erro
      }
    } catch (error) {
      console.error("Erro ao cadastrar cliente:", error);
      nameError.textContent = "Ocorreu um erro inesperado ao cadastrar."; // Exibe erro inesperado
    }
  }
});
// Estado inicial
continueButton.disabled = true;
nameError.textContent = "";