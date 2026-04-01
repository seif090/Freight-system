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
});
