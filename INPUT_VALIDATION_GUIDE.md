# Input Validation Guide - Limiting Text Fields

This guide shows how to enforce **minimal input constraints** on text fields across backend (DTOs) and frontend (Views).

---

## 1. Backend Validation (DTOs/Models)

### Using Data Annotations

```csharp
using System.ComponentModel.DataAnnotations;

public class ExampleDto
{
    // Short text field (e.g., codes, names)
    [Required(ErrorMessage = "Code is required")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Code must be 2-50 characters")]
    [RegularExpression(@"^[A-Z0-9_-]+$", ErrorMessage = "Only uppercase letters, numbers, _ and - allowed")]
    public string Code { get; set; } = string.Empty;

    // Standard name field
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be 2-200 characters")]
    public string Name { get; set; } = string.Empty;

    // Description/notes (longer but still limited)
    [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be 10-500 characters")]
    public string? Description { get; set; }

    // Comments (short)
    [StringLength(250, MinimumLength = 5, ErrorMessage = "Comment must be 5-250 characters")]
    public string? Comment { get; set; }

    // Email (standard validation)
    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    // Phone number
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone must be 10-20 digits")]
    [RegularExpression(@"^\+?[0-9\s-]+$", ErrorMessage = "Invalid phone format")]
    public string? Phone { get; set; }
}
```

### Common Length Limits by Field Type

| Field Type | Min Length | Max Length | Example |
|------------|------------|------------|---------|
| **Code/ID** | 2 | 50 | `WorkspaceCode`, `ControlNumber` |
| **Name** | 2 | 200 | `Name`, `Title` |
| **Short Description** | 5 | 250 | `Comment`, `Notes` |
| **Description** | 10 | 500 | `Description`, `Summary` |
| **Long Text** | 20 | 1000 | `DetailedDescription`, `Requirements` |
| **Email** | - | 255 | `Email` |
| **Phone** | 10 | 20 | `PhoneNumber` |
| **URL** | - | 500 | `Website`, `Link` |

---

## 2. Frontend Validation (Razor Views)

### HTML5 Attributes

```html
<!-- Short text input -->
<input type="text" 
       class="form-control" 
       name="Code" 
       id="Code"
       required
       minlength="2"
       maxlength="50"
       pattern="[A-Z0-9_-]+"
       title="Only uppercase letters, numbers, _ and - allowed">

<!-- Name field -->
<input type="text" 
       class="form-control" 
       name="Name" 
       id="Name"
       required
       minlength="2"
       maxlength="200"
       placeholder="Enter name (2-200 characters)">

<!-- Textarea with character counter -->
<div class="mb-3">
    <label for="Description" class="form-label">Description</label>
    <textarea class="form-control" 
              name="Description" 
              id="Description"
              rows="4"
              minlength="10"
              maxlength="500"
              placeholder="Enter description (10-500 characters)"
              oninput="updateCharCount(this, 'desc-counter', 500)"></textarea>
    <small class="text-muted">
        <span id="desc-counter">0</span>/500 characters
    </small>
    <div class="invalid-feedback">
        Description must be 10-500 characters
    </div>
</div>

<!-- Email with validation -->
<input type="email" 
       class="form-control" 
       name="Email" 
       id="Email"
       required
       maxlength="255"
       placeholder="user@example.com">
```

### Character Counter JavaScript

```javascript
function updateCharCount(textarea, counterId, maxLength) {
    const counter = document.getElementById(counterId);
    const current = textarea.value.length;
    counter.textContent = current;
    
    if (current > maxLength * 0.9) {
        counter.classList.add('text-warning');
    } else {
        counter.classList.remove('text-warning');
    }
    
    if (current > maxLength) {
        counter.classList.add('text-danger');
        textarea.classList.add('is-invalid');
    } else {
        counter.classList.remove('text-danger');
        textarea.classList.remove('is-invalid');
    }
}
```

---

## 3. Client-Side JavaScript Validation

### Real-time Validation

```javascript
// Add to your view or shared script
document.addEventListener('DOMContentLoaded', function() {
    // Get all inputs with maxlength
    const inputs = document.querySelectorAll('input[maxlength], textarea[maxlength]');
    
    inputs.forEach(input => {
        const maxLength = parseInt(input.getAttribute('maxlength'));
        const minLength = parseInt(input.getAttribute('minlength')) || 0;
        
        // Add character counter if it's a textarea
        if (input.tagName === 'TEXTAREA') {
            const counter = document.createElement('small');
            counter.className = 'text-muted d-block mt-1';
            counter.id = input.id + '-counter';
            counter.textContent = `0/${maxLength} characters`;
            input.parentNode.appendChild(counter);
            
            input.addEventListener('input', function() {
                const current = this.value.length;
                counter.textContent = `${current}/${maxLength} characters`;
                
                if (current < minLength) {
                    counter.classList.add('text-danger');
                    this.classList.add('is-invalid');
                } else if (current > maxLength) {
                    counter.classList.add('text-danger');
                    this.classList.add('is-invalid');
                } else {
                    counter.classList.remove('text-danger');
                    this.classList.remove('is-invalid');
                }
            });
        }
        
        // Prevent typing beyond maxlength
        input.addEventListener('input', function() {
            if (this.value.length > maxLength) {
                this.value = this.value.substring(0, maxLength);
            }
        });
        
        // Validate on blur
        input.addEventListener('blur', function() {
            if (this.value.length < minLength && this.hasAttribute('required')) {
                this.classList.add('is-invalid');
                this.setCustomValidity(`Minimum ${minLength} characters required`);
            } else {
                this.classList.remove('is-invalid');
                this.setCustomValidity('');
            }
        });
    });
});
```

---

## 4. Complete Example: Contact Form

### Backend DTO

```csharp
// Models/DTOs/ContactDto.cs
using System.ComponentModel.DataAnnotations;

namespace GrcMvc.Models.DTOs
{
    public class ContactRequestDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be 2-100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(255, ErrorMessage = "Email must not exceed 255 characters")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Subject is required")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Subject must be 5-100 characters")]
        public string Subject { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Message must be 10-1000 characters")]
        public string Message { get; set; } = string.Empty;
    }
}
```

### Frontend View

```html
<!-- Views/Help/Contact.cshtml -->
<form id="contactForm" method="post" asp-action="SubmitContact">
    <div class="mb-3">
        <label for="name" class="form-label">Name *</label>
        <input type="text" 
               class="form-control" 
               name="Name" 
               id="name"
               required
               minlength="2"
               maxlength="100"
               placeholder="Your name (2-100 characters)">
        <div class="invalid-feedback">Name must be 2-100 characters</div>
    </div>

    <div class="mb-3">
        <label for="email" class="form-label">Email *</label>
        <input type="email" 
               class="form-control" 
               name="Email" 
               id="email"
               required
               maxlength="255"
               placeholder="your.email@example.com">
        <div class="invalid-feedback">Please enter a valid email address</div>
    </div>

    <div class="mb-3">
        <label for="subject" class="form-label">Subject *</label>
        <input type="text" 
               class="form-control" 
               name="Subject" 
               id="subject"
               required
               minlength="5"
               maxlength="100"
               placeholder="Brief subject (5-100 characters)">
        <div class="invalid-feedback">Subject must be 5-100 characters</div>
    </div>

    <div class="mb-3">
        <label for="message" class="form-label">Message *</label>
        <textarea class="form-control" 
                  name="Message" 
                  id="message"
                  rows="5"
                  required
                  minlength="10"
                  maxlength="1000"
                  placeholder="Your message (10-1000 characters)"></textarea>
        <small class="text-muted">
            <span id="message-counter">0</span>/1000 characters
        </small>
        <div class="invalid-feedback">Message must be 10-1000 characters</div>
    </div>

    <button type="submit" class="btn btn-primary">Send Message</button>
</form>

<script>
    // Character counter for message
    const messageTextarea = document.getElementById('message');
    const messageCounter = document.getElementById('message-counter');
    
    messageTextarea.addEventListener('input', function() {
        const current = this.value.length;
        messageCounter.textContent = `${current}/1000`;
        
        if (current < 10) {
            messageCounter.classList.add('text-danger');
        } else if (current > 1000) {
            messageCounter.classList.add('text-danger');
            this.value = this.value.substring(0, 1000);
        } else {
            messageCounter.classList.remove('text-danger');
        }
    });
    
    // Form validation
    document.getElementById('contactForm').addEventListener('submit', function(e) {
        if (!this.checkValidity()) {
            e.preventDefault();
            e.stopPropagation();
        }
        this.classList.add('was-validated');
    });
</script>
```

---

## 5. Controller Validation

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SubmitContact(ContactRequestDto model)
{
    // ModelState is automatically populated from Data Annotations
    if (!ModelState.IsValid)
    {
        return View("Contact", model);
    }
    
    // Additional custom validation if needed
    if (model.Message.Contains("spam"))
    {
        ModelState.AddModelError(nameof(model.Message), "Message contains invalid content");
        return View("Contact", model);
    }
    
    // Process the request...
    return RedirectToAction("ThankYou");
}
```

---

## 6. Quick Reference: Common Patterns

### Pattern 1: Code/Identifier
```csharp
[StringLength(50, MinimumLength = 2)]
[RegularExpression(@"^[A-Z0-9_-]+$")]
```
```html
<input maxlength="50" minlength="2" pattern="[A-Z0-9_-]+">
```

### Pattern 2: Name/Title
```csharp
[StringLength(200, MinimumLength = 2)]
```
```html
<input maxlength="200" minlength="2">
```

### Pattern 3: Description
```csharp
[StringLength(500, MinimumLength = 10)]
```
```html
<textarea maxlength="500" minlength="10"></textarea>
```

### Pattern 4: Comment/Note
```csharp
[StringLength(250, MinimumLength = 5)]
```
```html
<textarea maxlength="250" minlength="5" rows="3"></textarea>
```

---

## 7. Best Practices

✅ **DO:**
- Set `maxlength` on ALL text inputs
- Use `minlength` for required fields
- Show character counters for textareas
- Validate on both client and server
- Provide clear error messages

❌ **DON'T:**
- Rely only on client-side validation
- Allow unlimited text input
- Skip validation on optional fields (still set maxlength)
- Use vague error messages

---

## 8. Implementation Checklist

- [ ] Add `[StringLength]` attributes to all DTO properties
- [ ] Add `maxlength` and `minlength` to all HTML inputs
- [ ] Add character counters for textareas
- [ ] Test validation on both client and server
- [ ] Verify error messages are user-friendly
- [ ] Check that existing data isn't truncated

---

## Next Steps

1. **Audit existing forms** - Find all text inputs without limits
2. **Update DTOs** - Add validation attributes
3. **Update Views** - Add HTML5 attributes
4. **Add JavaScript** - Character counters and real-time validation
5. **Test thoroughly** - Both valid and invalid inputs
