// Keeps a text field to digits (US-6 AC11). The thousands dots are added as the user types and
// are ignored when the value is read. A refused keystroke or paste
// is never silent: a short note beside the field says why (N1, N9).
//
// A paste is taken only when it reads as a whole Icelandic amount: digits, optionally grouped by
// dots, optionally followed by a decimal comma (dropped) and "kr.". "5.000.000 kr." gives
// 5000000 and "4.500.000,50" gives 4500000. Anything else ("4,500,000", "4500000.00") is refused
// rather than guessed at.

const digitCount = text => (text.match(/[0-9]/g) || []).length;
const icelandicAmount = /^(\d{1,3}(\.\d{3})+|\d+)(,\d*)?$/;
const noticeTime = 3000;

function digitsAfterReplacing(input, start, end, insertedDigits) {
    const kept = digitCount(input.value) - digitCount(input.value.slice(start, end));
    return kept + insertedDigits;
}

function wholeDigits(pasted) {
    const text = pasted.replace(/kr\.?/gi, '').replace(/\s/g, '');
    return icelandicAmount.test(text) ? text.split(',')[0].replace(/\./g, '') : null;
}

export function attach(input, maxDigits, notices) {
    const refusal = input.closest('.field')?.querySelector('.field-refusal');
    let clearTimer;

    const refuse = notice => {
        if (!refusal) return;
        refusal.textContent = notice;
        clearTimeout(clearTimer);
        clearTimer = setTimeout(() => { refusal.textContent = ''; }, noticeTime);
    };

    const accept = () => {
        if (!refusal) return;
        clearTimeout(clearTimer);
        refusal.textContent = '';
    };

    input.addEventListener('beforeinput', event => {
        if (input.readOnly) {
            return;
        }

        if (event.inputType === 'insertFromDrop') {
            event.preventDefault();
            refuse(notices.pasteRefused);
            return;
        }

        if (!event.inputType.startsWith('insert') || event.inputType === 'insertFromPaste' || event.data == null) {
            return;
        }

        const start = input.selectionStart ?? input.value.length;
        const end = input.selectionEnd ?? start;

        if (/[^0-9]/.test(event.data)) {
            event.preventDefault();
            refuse(event.data.includes(',') ? notices.noDecimals
                : event.data.includes('.') ? notices.dotsAreAdded
                : notices.digitsOnly);
        } else if (digitsAfterReplacing(input, start, end, event.data.length) > maxDigits) {
            event.preventDefault();
            refuse(notices.tooManyDigits);
        }
        // An accepted digit leaves a refusal note up for its full time: typing "7.5" must still
        // say why the dot went, now that the field reads 75.
    });

    // Where a field is long enough for thousands, the dots are put in as the user types. The
    // caret stays after the same digit, and the server reads the text with the dots ignored.
    if (notices.groupThousands) {
        input.addEventListener('input', () => {
            if (input.readOnly) {
                return;
            }

            const caret = input.selectionStart ?? input.value.length;
            const digitsBeforeCaret = digitCount(input.value.slice(0, caret));
            const grouped = input.value.replace(/[^0-9]/g, '').replace(/\B(?=(\d{3})+(?!\d))/g, '.');

            if (grouped === input.value) {
                return;
            }

            input.value = grouped;

            let position = 0;
            for (let seen = 0; position < grouped.length && seen < digitsBeforeCaret; position++) {
                if (/[0-9]/.test(grouped[position])) seen++;
            }
            input.setSelectionRange(position, position);
        });
    }

    input.addEventListener('paste', event => {
        event.preventDefault();

        if (input.readOnly) {
            return;
        }

        const digits = wholeDigits(event.clipboardData ? event.clipboardData.getData('text') : '');
        const start = input.selectionStart ?? input.value.length;
        const end = input.selectionEnd ?? start;

        if (digits === null || digits.length === 0) {
            refuse(notices.pasteRefused);
            return;
        }

        // A readable number that is too long for the field is told as that, not as "not a number".
        if (digitsAfterReplacing(input, start, end, digits.length) > maxDigits) {
            refuse(notices.tooManyDigits);
            return;
        }

        accept();
        input.setRangeText(digits, start, end, 'end');
        input.dispatchEvent(new Event('input', { bubbles: true }));
    });
}
