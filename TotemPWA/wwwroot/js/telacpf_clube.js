const cpfInput = document.querySelector('.inputCPF');
const confirmButton = document.querySelector('.cont');
const cpfError = document.getElementById('cpfError');

// Simulated valid CPFs - In a real application, this would be an API call
// Adicionei alguns CPFs para teste que são válidos pelo algoritmo, mas podem ser inválidos na "base de dados" simulada
const validCPFs = ["123.456.789-00", "987.654.321-00", "111.222.333-44", "555.666.777-88"]; 

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

// Melhoria na validação do algoritmo do CPF
function validateCPFAlgorithm(cpf) {
  cpf = cpf.replace(/\D/g, ''); // Garante que só números sejam usados para o algoritmo

  if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
    // Verifica se tem 11 dígitos e se todos são iguais (ex: 000.000.000-00, 111.111.111-11)
    return false;
  }

  let sum = 0;
  let remainder;

  for (let i = 1; i <= 9; i++) sum = sum + parseInt(cpf.substring(i-1, i)) * (11 - i);
  remainder = (sum * 10) % 11;

  if ((remainder == 10) || (remainder == 11))  remainder = 0;
  if (remainder != parseInt(cpf.substring(9, 10)) ) return false;

  sum = 0;
  for (let i = 1; i <= 10; i++) sum = sum + parseInt(cpf.substring(i-1, i)) * (12 - i);
  remainder = (sum * 10) % 11;

  if ((remainder == 10) || (remainder == 11))  remainder = 0;
  if (remainder != parseInt(cpf.substring(10, 11) ) ) return false;

  return true;
}

// Simula a validação de existência do CPF no backend
async function isValidCPFBackend(cpf) {
  try {
    // Faz uma requisição fetch real para o seu backend para verificar a existência do CPF
    const response = await fetch('/Admin/Client/CheckCpfExistence', { // Assumindo que este é o seu endpoint
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ cpf: cpf.replace(/\D/g, '') }) // Envia o CPF limpo
    });
    const data = await response.json();
    return data.exists; // Seu backend deve retornar { exists: true/false, clientName: "..." }
  } catch (error) {
    console.error("Erro ao verificar CPF no backend:", error);
    return false; // Assume que não foi encontrado em caso de erro
  }
}

async function validateAndSetButtonState(formattedCpf) {
  confirmButton.disabled = true; // Desabilita o botão por padrão 
  cpfError.textContent = ""; // Limpa erros anteriores 

  if (formattedCpf.length === 14) { // Verifica se o CPF está completo 
    const cleanCPF = formattedCpf.replace(/\D/g, ''); // Remove caracteres não numéricos 

    if (validateCPFAlgorithm(cleanCPF)) { // Valida o algoritmo do CPF 
      cpfError.textContent = "Verificando CPF..."; // Exibe mensagem de verificação 
      // Busca a existência e, potencialmente, o nome do cliente
      const response = await fetch('/Admin/Client/CheckCpfExistence', { // Requisição para o backend
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ cpf: cleanCPF })
      });
      const data = await response.json();
      const cpfExists = data.exists; // Obtém o status de existência do CPF
      const clientName = data.clientName; // Obtém o nome do cliente, se existir

      if (cpfExists) { // Se o CPF existir
        cpfError.textContent = `Bem-vindo(a), ${clientName}!`; // Exibe mensagem de boas-vindas
        confirmButton.disabled = false; // Habilita o botão
        // Armazenar clientName ou realizar qualquer ação com base no cliente existente
        // Por exemplo, redirecionar diretamente para 'SelecionarPedido' ou uma saudação personalizada
      } else { // Se o CPF não existir
        cpfError.textContent = "CPF não encontrado no clube. Por favor, cadastre-se."; // Mensagem de erro
        // Redireciona para a tela de registro de nome se o CPF não existir
        setTimeout(() => {
          window.location.href = 'TelaNomeClube'; // Redireciona para a entrada de nome 
        }, 1500); // Pequeno atraso antes de redirecionar
      }
    } else {
      cpfError.textContent = "CPF inválido."; // Mensagem de CPF inválido 
    }
  } else if (formattedCpf.length > 0) {
    cpfError.textContent = "CPF deve ter 11 dígitos."; // Mensagem de CPF incompleto 
  }
}


function onChange(input) {
  const formatted = formatCPF(input);
  cpfInput.value = formatted;
  keyboard.setInput(formatted);
  validateAndSetButtonState(formatted);
}

function onKeyPress(button) {
  // Não há shift/lock para teclado numérico
}
document.querySelector(".simple-keyboard").style.display = "block";


cpfInput.addEventListener("input", function () {
  const formatted = formatCPF(this.value);
  this.value = formatted;
  keyboard.setInput(formatted);
  validateAndSetButtonState(formatted);
});

// Event listener para o botão de confirmar (agora sem o <a> direto)
confirmButton.addEventListener("click", function(event) {
  if (!this.disabled) {
    // Se o botão não estiver desabilitado, navega para a próxima página
    window.location.href = 'SelecionarPedido';        
    // Impede a navegação se o botão estiver desabilitado (embora o disabled já cuide disso)
    event.preventDefault(); 
  }
});

// Estado inicial
confirmButton.disabled = true;
cpfError.textContent = "";