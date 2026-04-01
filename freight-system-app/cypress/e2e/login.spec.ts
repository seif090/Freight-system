describe('Auth + Customers + Shipments flow', () => {
  it('should login and load shipments list', () => {
    cy.visit('/login');
    cy.get('input[name="username"]').type('admin');
    cy.get('input[name="password"]').type('Admin123!');
    cy.get('button').contains('دخول').click();

    cy.url().should('contain', '/shipments');
    cy.contains('قائمة الشحنات');
  });

  it('should navigate to customers and open new customer form', () => {
    cy.visit('/customers');
    cy.contains('إنشاء عميل').click();
    cy.url().should('contain', '/customers/new');
    cy.get('input[name="name"]').type('Auto Customer');
    cy.get('input[name="email"]').type('auto@customer.test');
    cy.get('button').contains('حفظ').click();
  });
});
