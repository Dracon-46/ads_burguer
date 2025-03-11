function mostrarEsconder(id) {
    let sections = document.querySelectorAll('section');
    for (let i = 0; i < sections.length; i++) {
        sections[i].style.display = 'none';
    }
    document.getElementById(id).style.display = 'block';
}

function trocarCor(button) {
    
        let icons = document.querySelectorAll('.selection i');
        let spans = document.querySelectorAll('.selection span');
        icons.forEach(function(icon) {
            icon.style.backgroundColor = '';
            icon.style.color = '';
            icon.classList.remove('active');
        });
        
        spans.forEach(function(span) {
            span.style.backgroundColor = '';
            span.style.color = '';
            span.classList.remove('active');
           
        });

        let icon = button.querySelector('i');
        let span = button.querySelector('span');
        span.classList.add('active');
        icon.classList.add('active');
    

}
