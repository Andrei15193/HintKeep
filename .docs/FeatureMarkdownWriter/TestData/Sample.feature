@a
Feature: feature name
  Feature descirption

  As a user
  I want to document features
  So That I can review it later

  Background: scenario background
    Background descirption

    Given given background
      """
      Wall of text
      """
    And and background
      | column 1     | column 2     |
      | row 1 cell 1 | row 1 cell 2 |
      | row 2 cell 1 | row 2 cell 2 |
    But but background
    * item background
      """
      Wall of text
      """
    When when background
    Then then background

  @b @c
  Scenario: scenario name
  Scenario descirption

    Given given scenario
    When when scenario
    And and scenario
    But but scenario
    * item scenario
      | column 1     | column 2     |
      | row 1 cell 1 | row 1 cell 2 |
      | row 2 cell 1 | row 2 cell 2 |
    Then then scenario

  @d @e
  Scenario Outline: scenario outline
    Given given outline
    When when outline
    Then then outline
    And and outline
    But but outline
    * item outline

    Examples:
      | param |
      | value |

  @f @g
  Rule: rule
    Rule descirption

    Background: rule scenario background
      Rule background descirption

      Given given rule background
      And and rule background
      But but rule background
      * item rule background
      When when rule background
      Then then rule background

    @h @i
    Scenario: rule scenario name
    Rule scenario descirption

      Given given rule scenario
      When when rule scenario
      And and rule scenario
      But but rule scenario
      * item rule scenario
      Then then rule scenario

    @j @k
    Scenario Outline: rule scenario outline
      Given given rule outline
      When when rule outline
      Then then rule outline
      And and rule outline
      But but rule outline
      * item rule outline

      Examples:
        | param |
        | value |
