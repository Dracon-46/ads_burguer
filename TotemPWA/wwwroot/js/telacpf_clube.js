// telacpf_clube.js - Versão melhorada
const cpfInput = document.querySelector('.inputCPF');
const confirmButton = document.querySelector('.cont');
const cpfError = document.getElementById('cpfError');

// Configuração do teclado virtual
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

// Formatação do CPF
function formatCPF(value) {
  let cpf = value.replace(/\D/g, '');
  if (cpf.length > 11) cpf = cpf.slice(0, 11);

  cpf = cpf
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d)/, '$1.$2')
    .replace(/(\d{3})(\d{1,2})$/, '$1-$2');

  return cpf;
}

// Validação do algoritmo do CPF
function validateCPFAlgorithm(cpf) {
  cpf = cpf.replace(/\D/g, '');

  if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
    return false;
  }

  let sum = 0;
  let remainder;

  for (let i = 1; i <= 9; i++) sum = sum + parseInt(cpf.substring(i-1, i)) * (11 - i);
  remainder = (sum * 10) % 11;

  if ((remainder == 10) || (remainder == 11)) remainder = 0;
  if (remainder != parseInt(cpf.substring(9, 10))) return false;

  sum = 0;
  for (let i = 1; i <= 10; i++) sum = sum + parseInt(cpf.substring(i-1, i)) * (12 - i);
  remainder = (sum * 10) % 11;

  if ((remainder == 10) || (remainder == 11)) remainder = 0;
  if (remainder != parseInt(cpf.substring(10, 11))) return false;

  return true;
}

// Função principal de validação e definição do estado do botão
async function validateAndSetButtonState(formattedCpf) {
  confirmButton.disabled = true;
  cpfError.textContent = "";

  if (formattedCpf.length === 14) {
    const cleanCPF = formattedCpf.replace(/\D/g, '');

    if (validateCPFAlgorithm(cleanCPF)) {
      cpfError.textContent = "Verificando CPF...";
      
      try {
        const response = await fetch('/Admin/Client/CheckCpfExistence', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ cpf: cleanCPF })
        });
        
        const data = await response.json();
        
        if (data.exists) {
          // Cliente existe - verificar se é funcionário
          if (data.isEmployee) {
            cpfError.textContent = `Bem-vindo(a), ${data.clientName}! (Funcionário)`;
            cpfError.style.color = "green";
            confirmButton.disabled = false;
            
            // Armazenar informações do funcionário para próxima tela
            sessionStorage.setItem('currentUser', JSON.stringify({
              clientId: data.clientId,
              name: data.clientName,
              cpf: cleanCPF,
              isEmployee: true,
              employeeType: data.employeeType
            }));
          } else {
            cpfError.textContent = `Bem-vindo(a), ${data.clientName}! (Cliente)`;
            cpfError.style.color = "green";
            confirmButton.disabled = false;
            
            // Armazenar informações do cliente para próxima tela
            sessionStorage.setItem('currentUser', JSON.stringify({
              clientId: data.clientId,
              name: data.clientName,
              cpf: cleanCPF,
              isEmployee: false
            }));
          }
        } else {
          // Cliente não existe - precisa cadastrar
          cpfError.textContent = "CPF não encontrado. Redirecionando para cadastro...";
          cpfError.style.color = "orange";
          
          // Armazenar CPF para cadastro
          sessionStorage.setItem('newClientCpf', cleanCPF);
          
          setTimeout(() => {
            window.location.href = 'TelaNomeClube';
          }, 1500);
        }
      } catch (error) {
        console.error("Erro ao verificar CPF no backend:", error);
        cpfError.textContent = "Erro de conexão. Tente novamente.";
        cpfError.style.color = "red";
      }
    } else {
      cpfError.textContent = "CPF inválido.";
      cpfError.style.color = "red";
    }
  } else if (formattedCpf.length > 0) {
    cpfError.textContent = "CPF deve ter 11 dígitos.";
    cpfError.style.color = "red";
  }
}

// Handlers do teclado virtual
function onChange(input) {
  const formatted = formatCPF(input);
  cpfInput.value = formatted;
  keyboard.setInput(formatted);
  validateAndSetButtonState(formatted);
}

function onKeyPress(button) {
  // Não há shift/lock para teclado numérico
}

// Event listeners
cpfInput.addEventListener("input", function() {
  const formatted = formatCPF(this.value);
  this.value = formatted;
  keyboard.setInput(formatted);
  validateAndSetButtonState(formatted);
});

confirmButton.addEventListener("click", function(event) {
  event.preventDefault();
  if (!this.disabled) {
    window.location.href = 'SelecionarPedido';
  }
});

// Estado inicial
confirmButton.disabled = true;
cpfError.textContent = "";
document.querySelector(".simple-keyboard").style.display = "block";
