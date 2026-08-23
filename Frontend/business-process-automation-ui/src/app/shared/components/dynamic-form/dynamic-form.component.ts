import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { FormField } from '../../../forms/form.model';

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './dynamic-form.component.html'
})
export class DynamicFormComponent implements OnChanges {
  @Input({ required: true }) fields: FormField[] = [];
  @Input() submitLabel = 'Submit';
  @Input() submitting = false;
  @Output() formSubmit = new EventEmitter<Record<string, unknown>>();

  form = new FormGroup({});

  get sortedFields(): FormField[] {
    return [...this.fields].sort((a, b) => a.displayOrder - b.displayOrder);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fields']) {
      this.buildForm();
    }
  }

  private buildForm(): void {
    const group: Record<string, FormControl> = {};

    for (const field of this.fields) {
      const initialValue = field.fieldType === 'Checkbox' ? field.defaultValue === 'true' : field.defaultValue ?? '';
      group[field.fieldName] = new FormControl(initialValue, this.buildValidators(field));
    }

    this.form = new FormGroup(group);
  }

  private buildValidators(field: FormField): ValidatorFn[] {
    const validators: ValidatorFn[] = [];
    const rules = field.validationRules;

    if (field.isRequired) {
      validators.push(field.fieldType === 'Checkbox' ? Validators.requiredTrue : Validators.required);
    }
    if (field.fieldType === 'Email') {
      validators.push(Validators.email);
    }
    if (rules?.minLength != null) validators.push(Validators.minLength(rules.minLength));
    if (rules?.maxLength != null) validators.push(Validators.maxLength(rules.maxLength));
    if (rules?.min != null) validators.push(Validators.min(rules.min));
    if (rules?.max != null) validators.push(Validators.max(rules.max));
    if (rules?.pattern) validators.push(Validators.pattern(rules.pattern));

    return validators;
  }

  fieldControl(fieldName: string): FormControl {
    return this.form.get(fieldName) as FormControl;
  }

  hasError(fieldName: string): boolean {
    const control = this.form.get(fieldName);
    return !!control && control.invalid && control.touched;
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.formSubmit.emit(this.form.getRawValue());
  }
}
