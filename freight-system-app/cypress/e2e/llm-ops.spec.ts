describe('LLM Ops Dashboard', () => {
  it('should display a banner when manual populate is executed', () => {
    cy.visit('/analytics/llm-ops');

    cy.intercept('POST', '/api/analytics/manual-populate-delay-history', {
      statusCode: 200,
      body: { success: true }
    }).as('manualPopulate');

    cy.get('button').contains('Manual DelayHistory Refresh').click();

    cy.wait('@manualPopulate');

    cy.contains('Manual delay history refresh completed').should('be.visible');
  });

  it('should show financial summary and aging data', () => {
    cy.visit('/analytics/llm-ops');

    cy.get('h4').contains('Financial Summary').should('be.visible');
    cy.get('h4').contains('Invoice Aging').should('be.visible');

    cy.intercept('POST', '/api/v1.0/reports/invoices/1/mark-paid', {
      statusCode: 200,
      body: { success: true }
    }).as('markPaid');

    cy.get('button').contains('Mark top overdue invoice as Paid').click();

    cy.wait('@markPaid');

    cy.contains('Marked INV-1001 as paid (demo action)').should('be.visible');
  });
});
