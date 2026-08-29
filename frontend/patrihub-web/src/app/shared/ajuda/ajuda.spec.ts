import { TestBed } from '@angular/core/testing';

import { Ajuda } from './ajuda';

describe('Ajuda', () => {
  it('liga o botão de ajuda ao texto explicativo via aria-describedby', async () => {
    const fixture = TestBed.createComponent(Ajuda);
    fixture.componentRef.setInput(
      'texto',
      'Percentual de juros cobrado ao ano pelo financiamento.',
    );
    await fixture.whenStable();

    const botao: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    const descritoPorId = botao.getAttribute('aria-describedby');
    expect(descritoPorId).toBeTruthy();

    const balao: HTMLElement = fixture.nativeElement.querySelector(`#${descritoPorId}`);
    expect(balao.textContent?.trim()).toBe(
      'Percentual de juros cobrado ao ano pelo financiamento.',
    );
  });

  it('gera um id diferente por instância, pra não colidir quando há vários na mesma tela', async () => {
    const fixtureA = TestBed.createComponent(Ajuda);
    fixtureA.componentRef.setInput('texto', 'Explicação A');
    await fixtureA.whenStable();

    const fixtureB = TestBed.createComponent(Ajuda);
    fixtureB.componentRef.setInput('texto', 'Explicação B');
    await fixtureB.whenStable();

    const idA = fixtureA.nativeElement.querySelector('button').getAttribute('aria-describedby');
    const idB = fixtureB.nativeElement.querySelector('button').getAttribute('aria-describedby');
    expect(idA).not.toBe(idB);
  });

  it('não intercepta foco por teclado — é um botão comum, alcançável por Tab', async () => {
    const fixture = TestBed.createComponent(Ajuda);
    fixture.componentRef.setInput('texto', 'Explicação');
    await fixture.whenStable();

    const botao: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    expect(botao.type).toBe('button');
    expect(botao.tabIndex).not.toBe(-1);
  });

  it('mostra o balão ao focar e esconde ao perder o foco', async () => {
    const fixture = TestBed.createComponent(Ajuda);
    fixture.componentRef.setInput('texto', 'Explicação');
    await fixture.whenStable();

    const botao: HTMLButtonElement = fixture.nativeElement.querySelector('button');
    const balao: HTMLElement = fixture.nativeElement.querySelector('.ajuda-balao');
    expect(balao.classList.contains('ajuda-balao--visivel')).toBe(false);

    botao.dispatchEvent(new FocusEvent('focus'));
    await fixture.whenStable();
    expect(balao.classList.contains('ajuda-balao--visivel')).toBe(true);

    botao.dispatchEvent(new FocusEvent('blur'));
    await fixture.whenStable();
    expect(balao.classList.contains('ajuda-balao--visivel')).toBe(false);
  });

  it('o balão continua no DOM mesmo escondido, pro aria-describedby funcionar sem hover', async () => {
    const fixture = TestBed.createComponent(Ajuda);
    fixture.componentRef.setInput('texto', 'Percentual de juros cobrado ao ano.');
    await fixture.whenStable();

    const balao: HTMLElement = fixture.nativeElement.querySelector('.ajuda-balao');
    expect(balao.style.display).not.toBe('none');
    expect(balao.hidden).toBe(false);
    expect(balao.textContent?.trim()).toBe('Percentual de juros cobrado ao ano.');
  });
});
